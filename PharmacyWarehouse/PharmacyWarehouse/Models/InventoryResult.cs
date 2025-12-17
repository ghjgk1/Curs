using System;
using System.Collections.Generic;

namespace PharmacyWarehouse.Models;

public partial class InventoryResult : ObservableObject
{
    private int _id;
    private int _inventoryId;
    private int _productId;
    private int _accordingToData;
    private int _actual;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int InventoryId
    {
        get => _inventoryId;
        set => SetProperty(ref _inventoryId, value);
    }

    public int ProductId
    {
        get => _productId;
        set => SetProperty(ref _productId, value);
    }

    public int AccordingToData
    {
        get => _accordingToData;
        set => SetProperty(ref _accordingToData, value);
    }

    public int Actual
    {
        get => _actual;
        set => SetProperty(ref _actual, value);
    }

    public virtual Inventory Inventory { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
