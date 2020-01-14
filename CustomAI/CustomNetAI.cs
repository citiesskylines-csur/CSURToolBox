using ColossalFramework;
using ColossalFramework.Math;
using CSURToolBox.UI;
using CSURToolBox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CSURToolBox.CustomAI
{
    public class CustomNetAI: NetAI
    {
        public virtual float CustomGetCollisionHalfWidth()
        {
            // NON-STOCK CODE STARTS
            float laneOffset = 0;
            float startOffset = 0;
            float endOffset = 0;
            bool IsCSURSLane = CSURUtil.IsCSURSLane(m_info.m_netAI.m_info, ref laneOffset, ref startOffset, ref endOffset);
            if (CSURUtil.IsCSUROffset(m_info))
            {
                if (!IsCSURSLane)
                {
                    return (m_info.m_halfWidth - m_info.m_pavementWidth) / 2f;
                }
                else
                {
                    float laneNum = CSURUtil.CountCSURSVehicleLanes(m_info) + CSURUtil.CountCSURSOtherLanes(m_info);
                    return (laneNum * 3.75f / 2f);
                }
            }
            return m_info.m_halfWidth;
        }
	}
}
