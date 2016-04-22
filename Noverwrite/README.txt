Noverwrite by Hawkuro

No more overwriting of your saves!

Have the game back up your old save on a given day interval (can be every day)
at a set time in-game (only way to be sure game has finished saving). If you
have it set to not keep every single day, you can set the day offset as well.

Set up:
 - Drag the mod folder (Noverwrite folder containing Noverwrite.dll) into
   your [Stardew Valley folder]\Mods folder
 - (Optional) Configure the mod using config.json
 - Enjoy not worrying about your save being ocerwritten if you make a mistake

 Configuration:

 Open config.json in your favourite text editor and edit the values (right
 of the colons) to your liking.

 - SaveFolder: The location of your Stardew Valley save folder, usually keep
   this unchanged.
 - TimeToStore: The time of day to store your old save. Having it set to 610
   for example (RECOMMENDED) stores the previous day's save at 6:10 in the
   morning in-game. Storing saves at this sort of event ensures that they have
   finished properly saving. This is best to set as early as possible, but no
   earlier than 6:10. The game turns the clock to 6:00 before saving (and thus
   before moving the old save to where the mod can get it). It's also not
   recommended to go to sleep before the set time here as it can cause old
   saves to be overwritten, and we don't want that.
 - NoverwriteEveryXDays: Set how often the previous day's save is salvaged.
   E.g. setting it to 5 will be to Noverwrite every 5 days, i.e. every 5 days
   an extra save is created that will not be overwritten (while playing the
   current one).
 - NoverwriteDayOffset: The offset on when to save the previous day's save.
   Must be a number smaller than NoverwriteeveryXDays. E.g. if
   NoverwriteEveryXDays is 5 and this setting is 3, it'll save a previous
   day's save every five days starting with spring 4th (0 is spring 1st)
   year 1