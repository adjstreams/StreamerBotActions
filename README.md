# StreamerBotActions
A repo of custom code that has been written for StreamerBot c# Sub Actions on my stream.

You can get the fantastic StreamerBot from https://streamer.bot/

As well as the original code (because I want somewhere to maintain the source history) I've also included the import string so you can pull them into StreamerBot easily.

## Recommendation for importing actions in StreamerBot
When setting up events in streamerbot, never link imported actions directly to events in settings, as you will normally end up wanting to perform multiple actions in response to an event trigger. Instead create a series of empty actions and link link these in the various settings.

On First Words
On Chat Message
On New Raid
On New Host
On New Follower
On New Sub
On Cheer
etc

You can then trigger actions as sub-actions in these, and add to them as you need.

For example, for entrance sounds, you might:

1. Create a general action called "On First Words" and link this action to the First Words event via "Settings" > "Events" > "General" > "First Words"
2. Add the "Entrance Sounds" imported action and any others that you wish as a sub-action in this "On First Words" action.


