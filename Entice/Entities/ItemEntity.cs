using System;
using System.Collections.Generic;
using GuildWarsInterface;
using GuildWarsInterface.Datastructures.Agents;
using GuildWarsInterface.Datastructures.Agents.Components;
using GuildWarsInterface.Datastructures.Components;
using GuildWarsInterface.Datastructures.Items;
using GuildWarsInterface.Declarations;

namespace Entice.Entities
{
    internal class ItemEntity : Entity
    {
        public Item Item { get; private set; }
        public DroppedItem DroppedItem { get; private set; }

        private List<string> AttributeList = new List<string>();

        private bool FullySpecified {
            get
            {
                return AttributeList.Exists(attribute => attribute == "position") && AttributeList.Exists(attribute => attribute == "item");
            }
        }

        protected override void Initialized()
        {
            DroppedItem = new DroppedItem();
            //Item = new Item(ItemType.Dagger, 2147785208, "DroppedMOON", (ItemFlags)706876416, new ItemColor(Dye.Black), new ItemStat[] { new ItemStat(ItemStatIdentifier.WeaponRequirement, 0x1d, 0x09), new ItemStat(ItemStatIdentifier.DamageType, 0x02, 0x00), new ItemStat(ItemStatIdentifier.WeaponDamage, 0x11, 0x7) });
            //DroppedItem = new DroppedItem(Item);
        }

        protected override void Unload()
        {
            Game.Zone.RemoveAgent(DroppedItem);
        }

        protected override void UpdateAttribute(string name, dynamic value)
        {
            AttributeList.Add(name);
            switch (name)
            {
                case "position":
                    DroppedItem.Transformation.Position = new Position(float.Parse(value.x.ToString()), float.Parse(value.y.ToString()), short.Parse(value.plane.ToString()));
                    break;
                case "item":
                    if (!Enum.TryParse(value.type.ToString(), out ItemType type)) {
                        // not fully specified!
                        Debugging.Debug.Error("Received invalid ItemType '{0}'", value.type);
                        AttributeList.RemoveAt(AttributeList.Count - 1);
                        break;
                    }
                    if (!Enum.TryParse(value.model.ToString(), out ItemModel model)) { 
                        // not fully specified!
                        Debugging.Debug.Error("Received invalid ItemModel '{0}'", value.model);
                        AttributeList.RemoveAt(AttributeList.Count - 1);
                        break;
                    }
                    string item_name = value.name.ToString();
                    ItemFlags flags = 0;
                    foreach (object flag_string in value.flags)
                    {
                        if (Enum.TryParse(flag_string.ToString(), out ItemFlags flag))
                        {
                            flags |= flag;
                        }
                    }

                    List<ItemStat> itemStats = new List<ItemStat>();
                    foreach (object stat_string in value.stats)
                    {
                        string[] stat_string_parts = stat_string.ToString().Split(':');
                        bool success = true;
                        success &= Enum.TryParse(stat_string_parts[0], out ItemStatIdentifier statId);
                        success &= byte.TryParse(stat_string_parts[1], out var p1);
                        success &= byte.TryParse(stat_string_parts[2], out var p2);
                        if (success)
                        {
                            itemStats.Add(new ItemStat(statId, p1, p2));
                        }
                    }

                    Dye[] colors = new Dye[4];
                    int i = 0;
                    foreach (object color in value.color)
                    {
                        if (Enum.TryParse(color.ToString(), out Dye dye))
                        {
                            colors[i++] = dye;
                        }
                    }
                    ItemColor itemColor = new ItemColor(colors[0], colors[1], colors[2], colors[3]);

                    Item = new Item(type, (uint) model, item_name, flags, itemColor, itemStats);
                    DroppedItem.Item = Item;
                    break;
            }
            if (FullySpecified) {
                Game.Zone.AddAgent(DroppedItem);
            }
        }
    }
}
