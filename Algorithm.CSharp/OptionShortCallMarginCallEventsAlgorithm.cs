/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;

using QuantConnect.Data;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Algorithm asserting that the margin call events are fired when trading options
    /// </summary>
    public class OptionShortCallMarginCallEventsAlgorithm : OptionsMarginCallEventsAlgorithmBase
    {
        private Symbol _optionSymbol;

        protected override int OriginalQuantity => -10;
        protected override int ExpectedOrdersCount => 2;

        public override void Initialize()
        {
            SetStartDate(2015, 12, 23);
            SetEndDate(2015, 12, 30);
            SetCash(160000);

            var equitySymbol = AddEquity("GOOG").Symbol;

            var option = AddOption(equitySymbol);
            _optionSymbol = option.Symbol;

            option.SetFilter(u => u.Strikes(-2, +2)
                .Expiration(0, 180));

            Portfolio.MarginCallModel = new CustomMarginCallModel(Portfolio, DefaultOrderProperties);
        }

        public override void OnData(Slice slice)
        {
            if (!Portfolio.Invested)
            {
                if (IsMarketOpen(_optionSymbol) && slice.OptionChains.TryGetValue(_optionSymbol, out var chain))
                {
                    var callContracts = chain.Where(contract => contract.Right == OptionRight.Call)
                        .GroupBy(x => x.Expiry)
                        .OrderBy(grouping => grouping.Key)
                        .First()
                        .OrderByDescending(x => x.Strike)
                        .ToList();

                    MarketOrder(callContracts[0].Symbol, OriginalQuantity);
                }
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public override bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public override Language[] Languages { get; } = { Language.CSharp };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public override long DataPoints => 2973376;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public override int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public override Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "-0.07%"},
            {"Compounding Annual Return", "10.520%"},
            {"Drawdown", "1.400%"},
            {"Expectancy", "-1"},
            {"Net Profit", "0.210%"},
            {"Sharpe Ratio", "6.232"},
            {"Probabilistic Sharpe Ratio", "95.221%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0.078"},
            {"Beta", "-0.023"},
            {"Annual Standard Deviation", "0.013"},
            {"Annual Variance", "0"},
            {"Information Ratio", "1.1"},
            {"Tracking Error", "0.088"},
            {"Treynor Ratio", "-3.423"},
            {"Total Fees", "$3.50"},
            {"Estimated Strategy Capacity", "$66000.00"},
            {"Lowest Capacity Asset", "GOOCV W78ZFMML01JA|GOOCV VP83T1ZUHROL"},
            {"Portfolio Turnover", "1.01%"},
            {"OrderListHash", "c92fcc558f089ca06719642a66abb525"}
        };
    }
}
