using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Web.Responses;
using SteamKit2;

namespace ForgeYourFateASF
{
    [Export(typeof(IPlugin))]
    public class ForgeYourFate : IBotCommand
    {
        public string Name => nameof(ForgeYourFate);
        public Version Version => typeof(ForgeYourFate).Assembly.GetName().Version;

        public void OnLoaded()
        {
            ASF.ArchiLogger.LogGenericInfo("----------------------------------------");
            ASF.ArchiLogger.LogGenericInfo("                                        ");
            ASF.ArchiLogger.LogGenericInfo("             Forge Your Fate            ");
            ASF.ArchiLogger.LogGenericInfo("         Steam Summer Sale 2021         ");
            ASF.ArchiLogger.LogGenericInfo("                                        ");
            ASF.ArchiLogger.LogGenericInfo("----------------------------------------");
        }

        public async Task<string?> OnBotCommand(Bot bot, ulong steamID, string message, string[] args)
        {
            switch (args[0].ToUpperInvariant())
            {
                case "FORGEYOURFATE" when args.Length > 1:
                    return await ResponseForgeYourFate(steamID, Utilities.GetArgsAsText(args, 1, ",")).ConfigureAwait(false);
                case "FORGEYOURFATE":
                    return await ResponseForgeYourFate(bot, steamID).ConfigureAwait(false);
                default:
                    return null;
            }
        }

        private static async Task<string?> ResponseForgeYourFate(Bot bot, ulong steamID, int index = 0)
        {
            await Task.Delay(index * 250 + 1).ConfigureAwait(false);

            if (steamID == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(steamID));
            }

            if (!bot.HasAccess(steamID, BotConfig.EAccess.Master))
            {
                return null;
            }

            if (!bot.IsConnectedAndLoggedOn)
            {
                return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
            }

            Uri uriClaim = new(ArchiWebHandler.SteamStoreURL, "/promotion/ajaxclaimstickerforgenre");
            Summer2021Badge badge = Summer2021BadgeUtils.GetRandomSummer2021Badge();
            bot.ArchiLogger.LogGenericInfo($"Claiming {badge.Name} badge ...");

            for (int genre = 1; genre <= badge.Choices.Length; genre++)
            {
                await Task.Delay(100).ConfigureAwait(false);
                int choice = badge.Choices[genre - 1];
                Dictionary<string, string> data = new(3, StringComparer.Ordinal)
                {
                    { "genre", genre.ToString() },
                    { "choice", choice.ToString() }
                };

                ObjectResponse<ResultResponse>? response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<ResultResponse>(uriClaim, data: data).ConfigureAwait(false);
                if (response == null || response.Content.Result != EResult.OK)
                {
                    bot.ArchiLogger.LogGenericInfo($"Claim {badge.Name} badge Failed !!! Please try again later.");
                    return bot.Commands.FormatBotResponse(Strings.WarningFailed);
                }
            }

            bot.ArchiLogger.LogGenericInfo($"Claim {badge.Name} badge Success !!!");
            return bot.Commands.FormatBotResponse(Strings.Success); ;
        }

        private static async Task<string?> ResponseForgeYourFate(ulong steamID, string botNames)
        {
            if (steamID == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(steamID));
            }

            if (string.IsNullOrEmpty(botNames))
            {
                throw new ArgumentNullException(nameof(botNames));
            }

            HashSet<Bot>? bots = Bot.GetBots(botNames);

            if ((bots == null) || (bots.Count == 0))
            {
                return ASF.IsOwner(steamID) ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
            }

            IList<string?> results = await Utilities.InParallel(bots.Select((bot, index) => ResponseForgeYourFate(bot, steamID, index))).ConfigureAwait(false);

            List<string> responses = new(results.Where(result => !string.IsNullOrEmpty(result))!);

            return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
        }

    }
}
