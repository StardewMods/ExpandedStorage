namespace StardewMods.BetterChests.Framework.Models;

using System.Collections;
using Netcode;
using StardewValley.Inventories;
using StardewValley.Objects;

// ReSharper disable All
#pragma warning disable

/// <inheritdoc />
internal sealed class HeldItemsWrapper : IInventory
{
    private readonly StorageFurniture furniture;

    private int? cachedItemStackCount;
    private InventoryIndex? inventoryIndex;

    /// <summary>Initializes a new instance of the <see cref="HeldItemsWrapper" /> class.</summary>
    /// <param name="furniture">The storage furniture.</param>
    public HeldItemsWrapper(StorageFurniture furniture) => this.furniture = furniture;

    /// <inheritdoc />
    public int Count => this.Items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => this.Items.IsReadOnly;

    /// <inheritdoc />
    public long LastTickSlotChanged { get; }

    private NetObjectList<Item?> Items => this.furniture.heldItems;

    /// <inheritdoc />
    public Item? this[int index]
    {
        get => this.Items[index];
        set => this.Items[index] = value;
    }

    /// <inheritdoc />
    public void Add(Item? item) => this.Items.Add(item);

    /// <inheritdoc />
    public void AddRange(ICollection<Item> collection) => this.Items.AddRange(collection);

    /// <inheritdoc />
    public void Clear()
    {
        this.ClearIndex();
        this.Items.Clear();
    }

    /// <inheritdoc />
    public bool Contains(Item? item)
    {
        if (item == null)
        {
            return false;
        }

        return this.GetItemsById().TryGetMutable(item.QualifiedItemId, out var list) && list.Contains(item);
    }

    /// <inheritdoc />
    public bool ContainsId(string itemId)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId == null)
        {
            return false;
        }

        return this.GetItemsById().Contains(itemId);
    }

    /// <inheritdoc />
    public bool ContainsId(string itemId, int minimum)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId == null)
        {
            return false;
        }

        if (this.GetItemsById().TryGet(itemId, out var items))
        {
            if (minimum <= 1)
            {
                return true;
            }

            var count = 0;
            foreach (var item in items)
            {
                if (item.QualifiedItemId == itemId)
                {
                    count += item.Stack;
                }

                if (count >= minimum)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <inheritdoc />
    public void CopyTo(Item[] array, int arrayIndex) => this.Items.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public int CountId(string itemId)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId == null)
        {
            return 0;
        }

        if (this.GetItemsById().TryGet(itemId, out var items))
        {
            var count = 0;
            {
                foreach (var item in items)
                {
                    if (item.QualifiedItemId == itemId)
                    {
                        count += item.Stack;
                    }
                }

                return count;
            }
        }

        return 0;
    }

    /// <inheritdoc />
    public int CountItemStacks()
    {
        var itemStacksCount = this.cachedItemStackCount;
        if (itemStacksCount.HasValue)
        {
            return itemStacksCount.GetValueOrDefault();
        }

        var num = this.cachedItemStackCount = this.GetItemsById().CountItems();
        return num.Value;
    }

    /// <inheritdoc />
    public IEnumerable<Item> GetById(string itemId)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId == null || !this.GetItemsById().TryGet(itemId, out var items))
        {
            return Array.Empty<Item>();
        }

        return items;
    }

    /// <inheritdoc />
    public IEnumerator<Item> GetEnumerator() => this.Items.GetEnumerator();

    /// <inheritdoc />
    public IList<Item?> GetRange(int index, int count) => this.Items.GetRange(index, count);

    /// <inheritdoc />
    public bool HasAny() => this.GetItemsById().CountKeys() > 0;

    /// <inheritdoc />
    public bool HasEmptySlots() => this.Count > this.CountItemStacks();

    /// <inheritdoc />
    public int IndexOf(Item? item) => this.Items.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, Item? item) => this.Items.Insert(index, item);

    /// <inheritdoc />
    public void OverwriteWith(IList<Item?> list)
    {
        if (this == list || this.Items == list)
        {
            return;
        }

        this.ClearIndex();
        this.Items.CopyFrom(list);
    }

    /// <inheritdoc />
    public int ReduceId(string itemId, int count)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId == null)
        {
            return 0;
        }

        var itemsById = this.GetItemsById();
        if (itemsById.TryGetMutable(itemId, out var items))
        {
            var anyStacksRemoved = false;
            var remaining = count;
            for (var j = 0; j < items.Count; j++)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var item = items[j];
                var toRemove = Math.Min(remaining, item.Stack);
                items[j] = item.ConsumeStack(toRemove);
                if (items[j] == null)
                {
                    anyStacksRemoved = true;
                    item.Stack = 0;
                    items.RemoveAt(j);
                    j--;
                }

                remaining -= toRemove;
            }

            if (items.Count == 0)
            {
                itemsById.RemoveKey(itemId);
            }

            if (anyStacksRemoved)
            {
                for (var i = this.Items.Count - 1; i >= 0; i--)
                {
                    var item2 = this.Items[i];
                    if (item2 != null && item2.Stack == 0)
                    {
                        this.Items[i] = null;
                    }
                }
            }

            return count - remaining;
        }

        return 0;
    }

    /// <inheritdoc />
    public bool Remove(Item? item) => item != null && this.Items.Remove(item);

    /// <inheritdoc />
    public void RemoveAt(int index) => this.Items.RemoveAt(index);

    /// <inheritdoc />
    public bool RemoveButKeepEmptySlot(Item? item)
    {
        if (item == null)
        {
            return false;
        }

        var index = this.Items.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        this.Items[index] = null;
        return true;
    }

    /// <inheritdoc />
    public void RemoveEmptySlots()
    {
        if (!this.HasEmptySlots())
        {
            return;
        }

        for (var i = this.Count - 1; i >= 0; i--)
        {
            if (this[i] == null)
            {
                this.RemoveAt(i);
            }
        }
    }

    /// <inheritdoc />
    public void RemoveRange(int index, int count) => this.Items.RemoveRange(index, count);

    private void ClearIndex() => this.inventoryIndex = null;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.Items.GetEnumerator();

    /// <summary>Get an index of items by ID.</summary>
    private InventoryIndex GetItemsById() => this.inventoryIndex ??= InventoryIndex.ById(this.Items);
}