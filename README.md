# VWAP
A flexible VWAP indicator for cTrader

## Introduction
VWAP is a nice indicator for day trading. This version is a cTrader indicator with flexible Session selection using the Window and webview control provided by cTrader. 

## Features
- Customizable session start time and save it to the web local storage.
- Multiple session can be slected
- Ad hoc manual session for individual symbol
- Initialize once only, until reset.
![Session Selection Window](https://github.com/kenykau/VWAP/blob/main/Session%20Selection%20Window.png?raw=true "Session Selection Window")

## Mechanism
- Indicator Parameter: Reset Session, this parameter determines to show or skip the Session Dialog
- A txt file for the Session(s) selected will be saved in the Documents/cAlgo/Data/Indicator/VWAP folder for each symbol with respective timeframe. I.E. XAUUSD-H1.txt; or by changing the pattern in SessionHelper.cs 
  `string sessionFile => $"AIO_{_bars.SymbolName}_{_bars.TimeFrame.ShortName}.txt";`
