using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.Interfaces.Services;
using LibMatrix.Services;
using Microsoft.AspNetCore.Components;

namespace BugMine.Web.Classes;

public class BugMineStorage(
    ILogger<BugMineStorage> logger,
    IStorageProvider localStorage,
    HomeserverProviderService homeserverProviderService,
    NavigationManager navigationManager) {
    public async Task<List<UserAuth>?> GetAllTokens() {
        logger.LogTrace("Getting all tokens.");
        return await localStorage.LoadObjectAsync<List<UserAuth>>("bugmine.tokens") ??
               new List<UserAuth>();
    }

    public async Task<UserAuth?> GetCurrentToken() {
        logger.LogTrace("Getting current token.");
        var currentToken = await localStorage.LoadObjectAsync<UserAuth>("bugmine.token");
        var allTokens = await GetAllTokens();
        if (allTokens is null or { Count: 0 }) {
            await SetCurrentToken(null);
            return null;
        }

        if (currentToken is null) {
            await SetCurrentToken(currentToken = allTokens[0]);
        }

        if (!allTokens.Any(x => x.AccessToken == currentToken.AccessToken)) {
            await SetCurrentToken(currentToken = allTokens[0]);
        }

        return currentToken;
    }

    public async Task AddToken(UserAuth UserAuth) {
        logger.LogTrace("Adding token.");
        var tokens = await GetAllTokens() ?? new List<UserAuth>();

        tokens.Add(UserAuth);
        await localStorage!.SaveObjectAsync("bugmine.tokens", tokens);
    }

    private async Task<BugMineClient?> GetCurrentSession() {
        logger.LogTrace("Getting current session.");
        var token = await GetCurrentToken();
        if (token == null) {
            return null;
        }

        var hc = await homeserverProviderService.GetAuthenticatedWithToken(token.Homeserver, token.AccessToken, token.Proxy, useGeneric: true);
        return new BugMineClient(hc);
    }

    public async Task<BugMineClient?> GetSession(UserAuth userAuth) {
        logger.LogTrace("Getting session.");
        var hc = await homeserverProviderService.GetAuthenticatedWithToken(userAuth.Homeserver, userAuth.AccessToken, userAuth.Proxy, useGeneric: true);
        return new BugMineClient(hc);
    }

    public async Task<BugMineClient?> GetCurrentSessionOrNavigate() {
        logger.LogTrace("Getting current session or navigating.");
        var session = await GetCurrentSessionOrNull();
        
        if (session is null) {
            logger.LogInformation("No session found. Navigating to login.");
            navigationManager.NavigateTo("/Login");
        }

        return session;
    }

    public async Task<BugMineClient?> GetCurrentSessionOrNull() {
        BugMineClient? session = null;

        try {
            //catch if the token is invalid
            session = await GetCurrentSession();
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_UNKNOWN_TOKEN") {
                var token = await GetCurrentToken();
                logger.LogWarning("Encountered invalid token for {user} on {homeserver}", token.UserId, token.Homeserver);
                navigationManager.NavigateTo("/InvalidSession?ctx=" + token.AccessToken);
                return null;
            }

            throw;
        }

        return session;
    }

    public async Task RemoveToken(UserAuth auth) {
        logger.LogTrace("Removing token.");
        var tokens = await GetAllTokens();
        if (tokens == null) {
            return;
        }

        tokens.RemoveAll(x => x.AccessToken == auth.AccessToken);
        await localStorage.SaveObjectAsync("bugmine.tokens", tokens);
    }

    public async Task SetCurrentToken(UserAuth? auth) {
        logger.LogTrace("Setting current token.");
        await localStorage.SaveObjectAsync("bugmine.token", auth);
    }
}