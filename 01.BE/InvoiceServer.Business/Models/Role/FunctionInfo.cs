using InvoiceServer.Common.Constants;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace InvoiceServer.Business.Models
{
    public class FunctionInfo
    {
        private const int UnChecked = 0;
        private const int Checked = 1;
        private const int DisableAction = 2;

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("screenName")]
        public string ScreenName { get; set; }

        [JsonProperty("roleGroupId")]
        public long RoleGroupId { get; set; }

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

        public FunctionInfo()
        {

        }
        public FunctionInfo(string screenName, long id, char[] action)
        {
            this.ScreenName = screenName;
            this.Id = id;
            this.Read = action.Contains(CharacterAction.Read) ? Checked : DisableAction;
            this.Update = action.Contains(CharacterAction.Update) ? Checked : DisableAction;
            this.Create = action.Contains(CharacterAction.Create) ? Checked : DisableAction;
            this.Delete = action.Contains(CharacterAction.Delete) ? Checked : DisableAction;
            this.Active = action.Contains(CharacterAction.Active) ? Checked : DisableAction;
            this.Approve = action.Contains(CharacterAction.Approve) ? Checked : DisableAction;
            this.Sign = action.Contains(CharacterAction.Sign) ? Checked : DisableAction;
            this.Rejected = action.Contains(CharacterAction.Rejected) ? Checked : DisableAction;
            this.Pay = action.Contains(CharacterAction.Pay) ? Checked : DisableAction;
        }

        public void SetValue(char[] action)
        {
            Func<char, int> setAction = (charAction) =>
            {
                if (action.Contains(charAction))
                    return Checked;
                return UnChecked;
            };

            if (this.Approve != DisableAction)
            {
                this.Approve = setAction(CharacterAction.Approve);
            }

            if (this.Read != DisableAction)
            {
                this.Read = setAction(CharacterAction.Read);
            }

            if (this.Update != DisableAction)
            {
                this.Update = setAction(CharacterAction.Update);
            }

            if (this.Delete != DisableAction)
            {
                this.Delete = setAction(CharacterAction.Delete);
            }

            if (this.Create != DisableAction)
            {
                this.Create = setAction(CharacterAction.Create);
            }

            if (this.Active != DisableAction)
            {
                this.Active = setAction(CharacterAction.Active);
            }
            if (this.Rejected != DisableAction)
            {
                this.Rejected = setAction(CharacterAction.Rejected);
            }

            if (this.Sign != DisableAction)
            {
                this.Sign = setAction(CharacterAction.Sign);
            }

            if (this.Pay != DisableAction)
            {
                this.Pay = setAction(CharacterAction.Pay);
            }
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
