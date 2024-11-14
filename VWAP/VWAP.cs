using System;
using System.Runtime.Serialization;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Diagnostics;
using VWAPLib;
namespace cAlgo
{
    [Indicator(AccessRights = AccessRights.FullAccess, IsOverlay = true)]
    public class VWAP : Indicator
    {

        #region VWAP
        [Parameter("Reset Session", DefaultValue = false)]
        public bool ResetSessions { get; set; }
#if DEBUG
        [Parameter("Debug", DefaultValue = false)]
        public bool Debug { get; set; }
#endif
        [Output("VWAP", LineColor = "White", Thickness = 3)]
        public IndicatorDataSeries Result { get; set; }
        IndicatorDataSeries accVols, accPrices;
        SessionHelper helper;
        int lastIdx = -1;
        double P(int index) => (Bars[index].Close + Bars[index].High + Bars[index].Low) / 3;
        #endregion
        protected override void Initialize()
        {
#if DEBUG
            if (Debug)
                Debugger.Launch();
#endif
            #region VWAP
            helper = new SessionHelper(Bars, ResetSessions);
            helper.OnInitCompleted += Helper_OnInitCompleted;
            lastIdx = Bars.Count;
            accPrices = CreateDataSeries();
            accVols = CreateDataSeries();
            helper.Init();

            #endregion
        }
        private void Compute(int index)
        {
            var b = Bars[index];
            if (helper.IsNewSession(index))
            {
                accVols[index] = b.TickVolume;
                accPrices[index] = P(index) * b.TickVolume;

            }
            else
            {
                if (helper.IsReady)
                {
                    accVols[index] = accVols[index - 1] + b.TickVolume;
                    accPrices[index] = accPrices[index - 1] + P(index) * b.TickVolume;
                }
            }
            if (helper.IsReady)
            {
                Result[index] = accPrices[index] / accVols[index];
            }
        }
        private void Helper_OnInitCompleted(SessionHelperEventArgs obj)
        {
            for (int i = 0; i < Bars.Count; i++)
                Compute(i);
        }
        protected override void OnDestroy()
        {
            helper.Dispose();
            helper = null;
            base.OnDestroy();

        }

        public override void Calculate(int index)
        {

            if (helper.IsReady && index >= lastIdx)
                Compute(index);
        }
    }
}