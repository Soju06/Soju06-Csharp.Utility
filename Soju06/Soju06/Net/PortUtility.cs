/* ========= Soju06 Net Utility =========
 * NAMESPACE: Soju06.Net
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Net Utility ========= */
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Soju06.Net.Utility {
    public static class PortUtility {
        public const ushort MinUserPort = 1024;
        public const ushort MaxUserPort = 49151;

        /// <summary>
        /// 사용자 권한의 랜덤 포트를 가져옵니다.
        /// </summary>
        /// <returns>실패시 음수 반환</returns>
        public static int GetUserRandomPort() {
            List<int> usedPortList = new List<int>();
            foreach (var info in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
                usedPortList.Add(info.LocalEndPoint.Port);
            foreach (var point in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                usedPortList.Add(point.Port);
            for (int i = MinUserPort; i < MaxUserPort - MinUserPort; i++) {
                if (usedPortList.Contains(i)) continue;
                return i;
            }
            return -usedPortList.Count;
        }
    }
}
