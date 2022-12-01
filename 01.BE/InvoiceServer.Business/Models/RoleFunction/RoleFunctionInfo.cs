using InvoiceServer.Common.Constants;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace InvoiceServer.Business.Models
{
    public class RoleFunctionInfo
    {
        private const int UnChecked = 0;
        private const int Checked = 1;
        private const int DisableAction = 2;

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("screenName")]
        public string ScreenName { get; set; }

        [JsonProperty("roleId")]
        public long RoleId { get; set; }

        [JsonProperty("read")]
        public int Read { get; set; }

        [JsonProperty("update")]
        public int Update { get; set; }

        [JsonProperty("create")]
        public int Create { get; set; }

        [JsonProperty("delete")]
        public int Delete { get; set; }

        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("approve")]
        public int Approve { get; set; }

        [JsonProperty("sign")]
        public int Sign { get; set; }

        [JsonProperty("rejected")]
        public int Rejected { get; set; }

        [JsonProperty("pay")]
        public int Pay { get; set; }

        public RoleFunctionInfo()
        {

        }
        public RoleFunctionInfo(string screenName, long id, char[] permission, char[] actions)
        {
            Func<char[], char[], char, int> isGrant = (allActions, grantedPermission, action) =>
            {
                if (!allActions.Contains(action))
                    return DisableAction;
                if (grantedPermission.Contains(action))
                    return Checked;
                return UnChecked;
            };

            if (permission == null)
                permission = "".ToCharArray();
            if (actions == null)
                actions = "".ToCharArray();

            this.ScreenName = screenName;
            this.Id = id;
            this.Read = isGrant(actions, permission, CharacterAction.Read);
            this.Update = isGrant(actions, permission, CharacterAction.Update);
            this.Create = isGrant(actions, permission, CharacterAction.Create);
            this.Delete = isGrant(actions, permission, CharacterAction.Delete);
            this.Active = isGrant(actions, permission, CharacterAction.Active);
            this.Approve = isGrant(actions, permission, CharacterAction.Approve);
            this.Sign = isGrant(actions, permission, CharacterAction.Sign);
            this.Rejected = isGrant(actions, permission, CharacterAction.Rejected);
            this.Pay = isGrant(actions, permission, CharacterAction.Pay);
        }

        public string GetAction()
        {
            string action = string.Empty;
            if (this.Read == Checked)
            {
                action = action + CharacterAction.Read;
            }
            if (this.Update == Checked)
            {
                action = action + CharacterAction.Update;
            }

            if (this.Create == Checked)
            {
                action = action + CharacterAction.Create;
            }

            if (this.Delete == Checked)
            {
                action = action + CharacterAction.Delete;
            }

            if (this.Active == Checked)
            {
                action = action + CharacterAction.Active;
            }

            if (this.Approve == Checked)
            {
                action = action + CharacterAction.Approve;
            }

            if (this.Rejected == Checked)
            {
                action = action + CharacterAction.Rejected;
            }

            if (this.Sign == Checked)
            {
                action = action + CharacterAction.Sign;
            }

            if (this.Pay == Checked)
            {
                action = action + CharacterAction.Pay;
            }

            return action;
        }

    }
}
