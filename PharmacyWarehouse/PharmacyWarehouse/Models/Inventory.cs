using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PharmacyWarehouse.Models;

public partial class Inventory : ObservableObject
{
    private int _id;
    private DateOnly? _inventoryDate;
    private string _responsiblePerson = null!;
    private bool? _isCompleted;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public DateOnly? InventoryDate
    {
        get => _inventoryDate;
        set => SetProperty(ref _inventoryDate, value);
    }

    public string ResponsiblePerson
    {
        get => _responsiblePerson;
        set => SetProperty(ref _responsiblePerson, value);
    }

    public bool? IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    public virtual ICollection<InventoryResult> InventoryResults { get; set; } = new ObservableCollection<InventoryResult>();
}