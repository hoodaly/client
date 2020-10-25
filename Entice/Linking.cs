using Entice.Components;
using Entice.Definitions;
using GuildWarsInterface;
using GuildWarsInterface.Datastructures;
using GuildWarsInterface.Datastructures.Agents;
using GuildWarsInterface.Datastructures.Components;
using GuildWarsInterface.Declarations;
using GuildWarsInterface.Interaction;
using GuildWarsInterface.Logic;
using System.Collections.Generic;
using System.Linq;

namespace Entice
{
    internal static class Linking
    {
        public static void Initialize()
        {
            AuthLogic.Login = Login;
            AuthLogic.Logout = Networking.SignOut;
            AuthLogic.Play = Play;
            AuthLogic.AddFriend = (type, name, characterName) =>
                    {
                        switch (Networking.RestApi.AddFriend(name))
                        {
                            case AddFriendResult.Success:
                                Networking.UpdateFriends();
                                return true;

                            default:
                                return false;
                        }
                    };
            AuthLogic.MoveFriend = (name, target) =>
                    {
                        if (target != FriendList.Type.None) return false;
                        if (!Networking.RestApi.RemoveFriend(name)) return false;

                        Networking.UpdateFriends();
                        return true;
                    };
            AuthLogic.DeleteCharacter = (character) =>
                    {
                            Networking.RestApi.RemoveCharacter(character.Name);
                            return true;
                    };

            GameLogic.ChatMessage = ChatMessage;
            GameLogic.PartyInvite = invitee => Networking.Channels.Group.Merge(invitee);
            GameLogic.PartyKickInvite = party => Networking.Channels.Group.Kick(party.Leader);
            GameLogic.PartyAcceptJoinRequest = party => Networking.Channels.Group.Merge(party.Leader);
            GameLogic.PartyKickJoinRequest = party => Networking.Channels.Group.Kick(party.Leader);
            GameLogic.PartyKickMember = member => Networking.Channels.Group.Kick(member);
            GameLogic.PartyLeave = () => Networking.Channels.Group.Kick(Game.Player.Character);
            GameLogic.ExitToCharacterScreen = () => { if (Game.Zone.Loaded) Networking.Channels.All.ForEach(c => c.Leave()); };
            GameLogic.ExitToLoginScreen = Networking.SignOut;
            GameLogic.ChangeMap = map => Networking.Channels.Entity.MapChange(DefinitionConverter.ToArea(map));
            GameLogic.SkillBarEquipSkill = (slot, skill) => Networking.Channels.Skill.SkillbarSet(slot, skill);
            GameLogic.SkillBarMoveSkillToEmptySlot = (@from, to) =>
                    {
                        Skill skillTo = Game.Player.Character.SkillBar.GetSkill(@from);
                        Networking.Channels.Skill.SkillbarSet(@from, Skill.None);
                        Networking.Channels.Skill.SkillbarSet(to, skillTo);
                    };
            GameLogic.SkillBarSwapSkills = (slot1, slot2) =>
                    {
                        Skill skill1 = Game.Player.Character.SkillBar.GetSkill(slot1);
                        Skill skill2 = Game.Player.Character.SkillBar.GetSkill(slot2);

                        Networking.Channels.Skill.SkillbarSet(slot2, skill1);
                        Networking.Channels.Skill.SkillbarSet(slot1, skill2);
                    };
            GameLogic.CastSkill = (slot, target) => Networking.Channels.Skill.Cast(slot, target);
            GameLogic.ValidateNewCharacter = (name, apperance) => Networking.RestApi.CreateCharacter(name, apperance);
            GameLogic.ItemPickup = (droppedItem) => Networking.Channels.Entity.ItemPickup(droppedItem);
        }

        private static void ChatMessage(string message, Chat.Channel channel)
        {
            if (channel == Chat.Channel.Command)
            {
                Networking.Channels.Social.Emote(message);
            }
            else
            {
                Networking.Channels.Social.Message(message);
            }
        }

        private static bool Login(string email, string password, string character)
        {
            switch (Networking.SignIn(email, password))
            {
                case SecureRestApi.LoginResult.Error:
                    return false;

                case SecureRestApi.LoginResult.InvalidClientVersion:
                    Game.TemporaryFeatureREMOVE();
                    return false;
            }

            IEnumerable<PlayerCharacter> characters;
            if (!Networking.RestApi.GetCharacters(out characters)) return false;

            Game.Player.Account.ClearCharacters();
            characters.ToList().ForEach(Game.Player.Account.AddCharacter);
            Game.Player.Character = Game.Player.Account.Characters.FirstOrDefault();
            //Needs to be called before the interface sends the friendlist init messages or future updates won't work
            Networking.UpdateFriends();

            return true;
        }

        private static void Play(Map map)
        {
            Area area = DefinitionConverter.ToArea(map);

            Networking.ChangeArea(area, Game.Player.Character.Name);
        }
    }
}