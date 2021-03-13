/* ========= Soju06 Web Utility =========
 * NAMESPACE: Soju06.Web.Http.Utility
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Web Utility ========= */
using Soju06.Web.Json;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Soju06.Web.Utility {
    [DataContract]
    public class JWTToken<PAYLOAD> {
        public JWTToken() { }

        public JWTToken(string token) => DecodeToken(token);

        public JWTToken(string header, string payload, byte[] verifySignature) { 
            Header = header; PayloadString = payload; VerifySignature = verifySignature;
            SetBase64(); SetOriginal(); DecodePayload(); Init();
        }

        public JWTToken(string headerBase64, string payloadBase64, string verifySignatureBase64) {
            HeaderBase64 = headerBase64.Trim(); PayloadBase64 = payloadBase64.Trim(); 
            VerifySignatureBase64 = verifySignatureBase64.Trim();
            DecodeBase64(); SetOriginal(); DecodePayload(); Init();
        }

        private string _jwt;

        [DataMember(Name = "jwt")]
        public string JWT { get => _jwt; set => DecodeToken(value); }
        public string Header { get; private set; }
        public string HeaderBase64 { get; private set; }
        public PAYLOAD Payload { get; protected set; }
        public string PayloadString { get; private set; }
        public string PayloadBase64 { get; private set; }
        public byte[] VerifySignature { get; private set; }
        public string VerifySignatureBase64 { get; private set; }
        
        public bool IsComplete {
            get => !string.IsNullOrWhiteSpace(JWT) && !string.IsNullOrWhiteSpace(Header)
                && !string.IsNullOrWhiteSpace(HeaderBase64) && !string.IsNullOrWhiteSpace(PayloadString)
                && !string.IsNullOrWhiteSpace(PayloadBase64) && !string.IsNullOrWhiteSpace(VerifySignatureBase64);
        }

        protected virtual void SetBase64() {
            if (!string.IsNullOrWhiteSpace(Header))
                HeaderBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Header));
            if (!string.IsNullOrWhiteSpace(PayloadString))
                PayloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(PayloadString));
            if (VerifySignature?.Length > 0)
                VerifySignatureBase64 = Convert.ToBase64String(VerifySignature);
        }

        protected virtual void DecodeBase64() {
            if (!string.IsNullOrWhiteSpace(HeaderBase64))
                Header = Encoding.UTF8.GetString(FromBase64String(HeaderBase64));
            if (!string.IsNullOrWhiteSpace(PayloadBase64))
                PayloadString = Encoding.UTF8.GetString(FromBase64String(PayloadBase64));
            if (!string.IsNullOrWhiteSpace(VerifySignatureBase64))
                VerifySignature = FromBase64String(PayloadBase64);
        }

        protected virtual void DecodePayload() {
            if(!string.IsNullOrWhiteSpace(PayloadString))
                Payload = JsonUtility.Convert<PAYLOAD>(PayloadString);
        }

        protected virtual void SetOriginal() {
            _jwt = $"{HeaderBase64}.{PayloadBase64}.{VerifySignatureBase64}";
        }

        protected virtual void Init() {
            
        }

        protected bool DecodeToken(string token) {
            var os = token.Split('.');
            if (os.Length < 3) return false;
            HeaderBase64 = os[0].Trim(); PayloadBase64 = os[1].Trim();
            VerifySignatureBase64 = os[2].Trim();
            DecodeBase64(); _jwt = token; DecodePayload(); Init();
            return true;
        }

        private byte[] FromBase64String(string s) {
            s = s.Trim();
            int m = s.Length % 4;
            if (m > 0) s += new string('=', 4 - m);
            return Convert.FromBase64String(s);
        }

        /// <summary>
        /// string을 JWT 토큰으로 변환합니다.
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>토큰 방식이 올바르지 않을 경후 null를 반환합니다.</returns>
        public static JWTToken<PAYLOAD> DecodeJWTToken(string token) {
            try {
                var os = token.Split('.');
                if (os.Length < 3) return null;
                return new JWTToken<PAYLOAD>(os[0], os[1], os[2]);
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                return null;
            }
        }
    }

    [DataContract]
    public abstract class JWTTokenDefaultPayload {
        [DataMember(Name = "iat")]
        public long IssuedAt { get; set; }
        [DataMember(Name = "exp")]
        public long Expiraton { get; set; }
        [DataMember(Name = "iss")]
        public string Issuer { get; set; }

        public DateTime GetExpiratonTime() =>
            new DateTime(1970, 1, 1).AddSeconds(Expiraton);

        public DateTime GetIssuedAtTime() =>
            new DateTime().AddSeconds(Expiraton);
    }
}
