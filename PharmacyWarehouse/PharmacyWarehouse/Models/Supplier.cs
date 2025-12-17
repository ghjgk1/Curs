using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyWarehouse.Models;

public partial class Supplier : ObservableObject
{
    private int _id;
    private string _name = null!;
    private string? _phone;
    private string? _contactPerson;
    private string? _address;
    private bool _isActive = true;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string? Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string? ContactPerson
    {
        get => _contactPerson;
        set => SetProperty(ref _contactPerson, value);
    }

    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }


    public virtual ICollection<Batch> Batches { get; set; } = new ObservableCollection<Batch>();
    public virtual ICollection<Document> Documents { get; set; } = new ObservableCollection<Document>();

    [NotMapped]
    public int BatchesCount => Batches?.Count ?? 0;

    [NotMapped]
    public int DocumentsCount => Documents?.Count ?? 0;
}