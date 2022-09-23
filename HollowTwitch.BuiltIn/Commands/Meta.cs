using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Meta : CommandBase
    {
        [OwnerOnly]
        [HKCommand("echo")]
        [Summary("Echoes a message.")]
        public void Echo([RemainingText] string msg)
        {
            Logger.Log($"Sending message \'{msg}\'.");
            SendMessage(msg);
        }

        [OwnerOnly]
        [HKCommand("blacklist")]
        [Summary("Disable a command's usage.")]
        public void Blacklist(string command)
        {
            Logger.Log($"Requested blacklist of command {command}");

            List<string> blacklist = TwitchMod.Instance.Config.BlacklistedCommands;

            if (!CommandExists(command))
                return;

            if (blacklist.Contains(command))
            {
                Logger.LogWarn($"Command {command} was already in the blacklist.");
                return;
            }

            blacklist.Add(command);
        }

        [OwnerOnly]
        [HKCommand("allowCommand")]
        [Summary("Re-enable a command's usage after having blacklisted it.")]
        public void AllowCommand(string command)
        {
            List<string> blacklist = TwitchMod.Instance.Config.BlacklistedCommands;

            if (!CommandExists(command))
                return;

            if (!blacklist.Contains(command, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogWarn($"Command {command} was not in blacklist!");

                return;
            }

            blacklist.RemoveAll(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
        }

        [OwnerOnly]
        [HKCommand("ban")]
        [Summary("Prevent a user from using commands.")]
        public void Ban(string user)
        {
            Logger.Log($"Banning user {user}.");

            List<string> users = TwitchMod.Instance.Config.BannedUsers;

            if (!users.Contains(user))
                users.Add(user);
        }

        [OwnerOnly]
        [HKCommand("unban")]
        [Summary("Re-allow a user to use commands.")]
        public void Unban(string user)
        {
            Logger.Log($"Unbanning user {user}.");

            List<string> users = TwitchMod.Instance.Config.BannedUsers;

            if (!users.Contains(user))
            {
                Logger.LogWarn($"User {user} is not banned!");

                return;
            }

            users.RemoveAll(u => u.Equals(user, StringComparison.OrdinalIgnoreCase));
        }
        
        [OwnerOnly]
        [HKCommand("timeout")]
        [Summary("Timeout a user or command for a specified time (seconds).")]
        public IEnumerator Timeout(string item, float time)
        {
            bool is_command = CommandExists(item);

            if (is_command)
                Blacklist(item);
            else
                Ban(item);

            yield return new WaitForSecondsRealtime(time);
            
            if (is_command)
                AllowCommand(item);
            else
                Unban(item);
        }

        private static bool CommandExists(string command)
        {
            return TwitchMod.Instance.Processor.Commands.Select(x => x.Name).Contains(command, StringComparer.OrdinalIgnoreCase);
        }
    }
}