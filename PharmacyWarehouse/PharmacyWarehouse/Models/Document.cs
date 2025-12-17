using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PharmacyWarehouse.Models;

public partial class Document : ObservableObject
{
    private int _id;
    private string _number = null!;
    private string _type = null!;
    private DateOnly? _date;
    private int? _supplierId;
    private string? _counterparty;
    private decimal? _amount;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public DateOnly? Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public int? SupplierId
    {
        get => _supplierId;
        set => SetProperty(ref _supplierId, value);
    }

    public string? Counterparty
    {
        get => _counterparty;
        set => SetProperty(ref _counterparty, value);
    }

    public decimal? Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public virtual ICollection<DocumentLine> DocumentLines { get; set; } = new ObservableCollection<DocumentLine>();
    public virtual ICollection<MovementJournal> MovementJournals { get; set; } = new ObservableCollection<MovementJournal>();
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<WriteOff> WriteOffs { get; set; } = new ObservableCollection<WriteOff>();
}