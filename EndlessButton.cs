using UnityEngine;
using Npc.Parts;

namespace EndlessDays
{
    class EndlessButton
    {
        public class EndlessButtonClick : InteractiveItem
        {

            public void OnMouseDown()
            {
                if(!Plugin.endlessMode)
                {
                    // Grab sprite for button and change it
                    var sr = Plugin.endlessButtonGO.GetComponent<SpriteRenderer>();
                    sr.sprite = Plugin.endlessButtonOnSprite;

                    // Show a notification
                    Notification.ShowText("Endless Days mode on", "Customers for evermore...", Notification.TextType.EventText);

                    // Grab the name of the room
                    string roomName = Managers.Room.settings.rooms[(int)Managers.Room.currentRoom].name;

                    // If we're in the bedroom, grab some customers
                    if(roomName == "BedroomRoom")
                    {
                        Day getDay = Managers.Day.settings.groundhogDay;
                        Managers.Environment.ResetNpcCounters();
                        Managers.Npc.ClearNpcQueue();
                        foreach (DailyVisitor dailyVisitor in getDay.templatesToSpawn)
                        {
                            Managers.Npc.AddToQueueForSpawn(dailyVisitor, false);
                        }
                    }

                    // Turn endless mode on
                    Plugin.endlessMode = true;
                }
                else
                {
                    // Grab sprite for button and change it
                    var sr = Plugin.endlessButtonGO.GetComponent<SpriteRenderer>();
                    sr.sprite = Plugin.endlessButtonOffSprite;

                    // Show a notification
                    Notification.ShowText("Endless Days mode off", "Customers will now stop queueing ...", Notification.TextType.EventText);

                    // Turn endless mode off
                    Plugin.endlessMode = false;
                }
            }
        }
    }
}
