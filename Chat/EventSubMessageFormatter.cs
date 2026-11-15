using CatCore.Models.Twitch.EventSub.Channel;
using CatCore.Models.Twitch.EventSub.ChannelPoints;
using CatCore.Models.Twitch.EventSub.Charity;
using CatCore.Models.Twitch.EventSub.Goal;
using CatCore.Models.Twitch.EventSub.HypeTrain;
using CatCore.Models.Twitch.EventSub.Moderation;
using System.Globalization;

namespace EnhancedStreamChat.Chat
{
    /// <summary>
    /// Builds the system message text shown in chat for the EventSub events added in CatCore 2.0.0-alpha2.
    /// </summary>
    internal static class EventSubMessageFormatter
    {
        public static string FormatHypeTrainBegin(HypeTrainEvent data)
        {
            return $"🚂 Hype Train started! (Level {data.Level})";
        }

        public static string FormatHypeTrainEnd(HypeTrainEvent data)
        {
            return $"🚂 Hype Train ended at Level {data.Level} (Total: {FormatNumber(data.Total)})";
        }

        public static string FormatGoalBegin(GoalEvent data)
        {
            return $"🎯 Goal started: {FormatGoal(data)}";
        }

        public static string FormatGoalEnd(GoalEvent data)
        {
            var result = data.IsAchieved == true ? "achieved" : "not achieved";
            return $"🎯 Goal ended: {FormatGoal(data)} ({result})";
        }

        public static string FormatCharityCampaignStarted(CharityCampaignEvent data)
        {
            return $"💖 Charity campaign started: {FormatCharity(data.CharityName)} ({FormatAmount(data.CurrentAmount)} / {FormatAmount(data.TargetAmount)})";
        }

        public static string FormatCharityCampaignStopped(CharityCampaignEvent data)
        {
            return $"💖 Charity campaign ended: {FormatCharity(data.CharityName)} ({FormatAmount(data.CurrentAmount)} raised)";
        }

        public static string FormatShoutoutReceived(ShoutoutReceivedEvent data)
        {
            var name = DisplayName(data.FromBroadcasterUserName, data.FromBroadcasterUserLogin);
            return data.ViewerCount > 0
                ? $"📣 {name} sent a shoutout! ({FormatNumber(data.ViewerCount)} viewers)"
                : $"📣 {name} sent a shoutout!";
        }

        public static string FormatUserBanned(BanEvent data)
        {
            var name = DisplayName(data.UserName, data.UserLogin);
            var moderator = DisplayName(data.ModeratorUserName, data.ModeratorUserLogin);
            var action = data.IsPermanent ? "banned" : "timed out";
            var message = $"🔨 {name} was {action} by {moderator}";
            return string.IsNullOrWhiteSpace(data.Reason) ? message : $"{message} (reason: {data.Reason})";
        }

        public static string FormatUserUnbanned(UnbanEvent data)
        {
            var name = DisplayName(data.UserName, data.UserLogin);
            var moderator = DisplayName(data.ModeratorUserName, data.ModeratorUserLogin);
            return $"🔓 {name} was unbanned by {moderator}";
        }

        public static string FormatAutomaticRewardRedeemed(AutomaticRewardRedemptionEvent data)
        {
            var name = DisplayName(data.UserName, data.UserLogin);
            var message = $"✨ {name} used {FormatRewardType(data.Reward.Type)}";
            return data.Reward.Emote is { } emote && !string.IsNullOrEmpty(emote.Name) ? $"{message} ({emote.Name})" : message;
        }

        public static string FormatCustomPowerUpRedeemed(CustomPowerUpRedemptionEvent data)
        {
            var name = DisplayName(data.UserName, data.UserLogin);
            var title = string.IsNullOrWhiteSpace(data.CustomPowerUp.Title) ? "a custom Power-up" : data.CustomPowerUp.Title;
            var message = $"⚡ {name} used {title} ({FormatNumber(data.CustomPowerUp.Bits)} bits)";
            return string.IsNullOrWhiteSpace(data.UserInput) ? message : $"{message}: {data.UserInput}";
        }

        private static string FormatGoal(GoalEvent data)
        {
            var description = string.IsNullOrWhiteSpace(data.Description)
                ? (string.IsNullOrEmpty(data.Type) ? "Goal" : data.Type.Replace('_', ' '))
                : data.Description;
            return $"{description} ({FormatNumber(data.CurrentAmount)}/{FormatNumber(data.TargetAmount)})";
        }

        private static string FormatCharity(string charityName)
        {
            return string.IsNullOrWhiteSpace(charityName) ? "Charity" : charityName;
        }

        private static string FormatRewardType(string type)
        {
            return type switch {
                "gigantify_an_emote" => "Gigantify an Emote",
                "message_effect" => "Message Effect",
                "celebration" => "On-Screen Celebration",
                _ => string.IsNullOrEmpty(type) ? "a Power-up" : type.Replace('_', ' '),
            };
        }

        private static string FormatAmount(CharityAmount amount)
        {
            var value = (decimal)amount.Value;
            for (var i = 0; i < amount.DecimalPlaces; i++) {
                value /= 10m;
            }
            var text = value.ToString("0.##", CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(amount.Currency) ? text : $"{text} {amount.Currency}";
        }

        private static string FormatNumber(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string DisplayName(string name, string login)
        {
            if (!string.IsNullOrWhiteSpace(name)) {
                return name;
            }
            return string.IsNullOrWhiteSpace(login) ? "Unknown user" : login;
        }
    }
}
