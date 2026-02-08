using System.Collections.Generic;
using System.Threading.Tasks;
using ESI.NET;
using ESI.NET.Enumerations;
using ESI.NET.Models.SSO;
using Microsoft.Extensions.Options;

namespace EveMiningFleet.Logic.EsiEve
{
    public class EveEsiConnexion
    {
        public EsiClient EsiClient;
        public SsoToken ssoToken { get; set; }
        public AuthorizedCharacterData authorizedCharacterData { get; set; }

        // Variable locale pour stocker une référence vers l'instance

        public EveEsiConnexion()
        {
            try
            {
                IOptions<EsiConfig> config;

                config = Options.Create(new EsiConfig()
                {
                    EsiUrl = "https://esi.evetech.net/",
                    DataSource = DataSource.Tranquility,
                    ClientId = System.Environment.GetEnvironmentVariable("EveESIClientId"),
                    SecretKey = System.Environment.GetEnvironmentVariable("EveESISecretKey"),
                    CallbackUrl = System.Environment.GetEnvironmentVariable("EveESICallbackUrl"),
                    UserAgent = "EveMiningFleet"
                });

                EsiClient = new EsiClient(config);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }

        public string GetUrlConnection(string state = "0")
        {
            List<string> scopes = new List<string>() { "esi-industry.read_character_mining.v1" };
            return EsiClient.SSO.CreateAuthenticationUrl(scopes, state);
        }

        public async Task GetToken(string acodeEsiReturn)
        {
            try
            {
                this.ssoToken = await this.EsiClient.SSO.GetToken(GrantType.AuthorizationCode, acodeEsiReturn);
                await ConnectCharCCP();
            }
            catch
            {
                this.ssoToken = null;
            }
        }
        public async Task RefreshConnection(string arefreshtoken)
        {
            try
            {
                this.ssoToken = await this.EsiClient.SSO.GetToken(GrantType.RefreshToken, arefreshtoken);
                await ConnectCharCCP();
            }
            catch
            {
                this.ssoToken = null;
            }
        }
        public async Task ConnectCharCCP()
        {
            try
            {
                this.authorizedCharacterData = await this.EsiClient.SSO.Verify(this.ssoToken);
                this.EsiClient.SetCharacterData(this.authorizedCharacterData);
            }
            catch
            {
                this.authorizedCharacterData = null;
            }
        }






    }
}