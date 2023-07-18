# Better Trader Remake

<hr />

<b>Notice</b>: Menthus is working on a rewrite (2023.06.12).

<hr />
<p align="center">
<b>Digitalroot and Menthus can be found in the Valhalla Legends Discord</b><br /><br />
  <a href="https://discord.gg/SsMW3rm67u" target="_blank"><img src="https://digitalroot.net/img/vl/vl_logo_125x154.png"></a>
</p>


Permission to fix and upload granted by Menthus.

<hr />

## Summary
Adds a custom trader menu, 310+ items to buy/sell, built-in support for Epic Loot, and an extensive list of configuration options.

## Patreon
I spend countless hours every day working on, updating, and fixing mods for everyone to enjoy. While I will never ask for anyone to pay me to make a mod or add a feature, any support is greatly appreciated. >>[Check it out?](https://www.patreon.com/Menthus)<<, you might find benefits that catch your eye!

## Configuration
Included with the mod are two config files. 
 - <b>Menthus.bepinex.plugins.BetterTrader.cfg.vanilla</b> closely emulates the vanilla values for buying and selling items.
 - <b>Menthus.bepinex.plugins.BetterTrader.cfg.full</b> has most of the game items in it with the values Menthus set for them.

Select one of these files and copy it to <i>Valheim\BepInEx\config</i>, <u>then</u> rename it to <b>Menthus.bepinex.plugins.BetterTrader.cfg</b>

###### You have two methods of editing the configuration:

1. Going to Valheim\BepInEx\config and opening Menthus.bepinex.plugins.BetterTrader.cfg.
2. Using the >>[BepInEx ConfigurationManager?](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.1)<< plugin. Simply download it, put it in your Valheim\BepInEx\plugins folder, and press F1 when in-game.

**You have a growing list of configuration options:**

#### [A_General.Trader]
**Trader Wait For Discovery (defaults to true):** True means the Trader will only sell items the player has discovered.  
**Trader Price Fluctuation (defaults to true):** True means the Trader's prices will fluctuate every so often (the amount to fluctuate by and time it takes before the Trader's prices fluctuate are configurable).  
**Trader Price Fluctuation Scale (defaults to 0.3):** Determines how much the prices should fluctuate from the base price of an item.  
**Trader Price Fluctuation Frequency (defaults to 1):** How many days that pass between each price fluctuation.  
###### Note: Price Fluctuation has been reported to not be working as intended
**Trader Has Coins (defaults to true):** True means the Trader will use coins to purchase player items (the Trader's starting coin amount and time it takes before the Trader's coin reset are configurable).  
**Trader Base Coin Amount (defaults to 2000):** How many coins the Trader has as a baseline.  
**Trader Coin Refresh Frequency (defaults to 2):** How many days need to pass before refreshing the Trader's coin amount to [Trader Base Coin Amount].  
**Trader Can Repair Items (defaults to true):** True means the Trader can repair items for the Player.  

#### [B_General.ItemTypes.NameOfItemType]
**enabled (defaults to true):** True means every item of this type is enabled.  

#### [C_Items.NameOfItemType.NameOfItem]
**Purchase Price (defaults to 100):** The base amount the Trader sells this item for (before fluctuating/scaling if [Trader Price Fluctuation] is true).  
**Sell Price (defaults to 100):** The base amount this item can be sold for by the Player.  
**Tradeable (defaults to false):** True means the Trader will sell this item to the Player.  
**Sellable (defaults to false):** True means the Player can sell this item to the Trader.  
**Ignore Trader Wait For Discovery (defaults to false):** True means this item will show up in the Trader's purchase list even if [Trader Wait For Discovery] is true and this item hasn't been discovered by the Player yet.  

**Example of an item's price being fluctuated**  
    ?priceFluctuationFactor = basePrice * TraderPriceFluctuationScale;
    newPrice = basePrice + Random.Range(-priceFluctuationFactor, priceFluctuationFactor);

## Installation (manual):
1. This mod requires >>[BepInExPack](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.1)<< to work. Follow the BepInExPack installation instructions, then place the BepInEx folder (from the Better Trader download) into the Valheim folder. Make sure to replace all if you have a previous version of Better Trader installed!
2. If you're playing the game in a different language **and you are missing items**, you must run the game in English and load into a world first. Then you can swap it back to your language and continue from there.

## ChangeLog

### Better  Trader 2.2.10
- Update for 0.216.9

### Better  Trader 2.2.9
- Update for 0.214.300
- Fixed Shader issue

### Better  Trader 2.2.8
- Improved compatibility with Epic Valheim Mod

### Better  Trader 2.2.7
- Added Egg to trader

### Better  Trader 2.2.6
- Asset file is now embedded

### Better  Trader 2.2.5
- Removed ref to JVL
- Unfiltered EpicLoot items

### Better  Trader 2.2.4
- Fixed error for missing worlds_local directory
- Added Megingjord to the vanilla config. (Menthus.bepinex.plugins.BetterTrader.cfg.vanilla)

### Better  Trader 2.2.3
- Merged the config from 2.1.3 with 2.2.2
- Added new HnH and Mistlands items to the config. 

### Better  Trader 2.2.2
- Added missing asset file

### Better  Trader 2.2.0
- Updated for Mistlands
- Fixed Trader Coin issue
- Deprecated "Is Server" config
- Updated default config to emulate the vanilla values for buying and selling items.

### Better  Trader 2.1.3
- Updated for HnH

### Better  Trader 2.1.2
- Fix for Horn of celebration causing NRE

### Better trader 2.0.4
- Fix where NullReferenceException would be thrown upon closing the game.
- Fix for equipped indicator would show as bt_equipped instead of Equipped.
- General refactoring.
- Added missing config to zip files.

### Better trader 2.0.3
- Fix for mod enforcement.
- Fix for config syncing.

### Better trader 2.0.2
- Fix for config not syncing.
- General refactoring.

## Links

**GPORTAL**  
http://www.g-portal.com/?ref=Menthus15

**Patreon**  
https://www.patreon.com/Menthus

**Youtube**  
https://www.youtube.com/channel/UCfVYisJ8c6p37ol154rTHnw

**Twitter**  
https://twitter.com/OriginalMenthus
