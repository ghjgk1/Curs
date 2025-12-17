using System;
using System.Collections.Generic;

namespace PharmacyWarehouse.Models;

public partial class Notification : ObservableObject
{
    private int _id;
    private string? _type;
    private int? _productId;
    private int? _batchId;
    private string? _message;
    private DateTime? _createdDate;
    private bool? _isRead;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string? Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public int? ProductId
    {
        get => _productId;
        set => SetProperty(ref _productId, value);
    }

    public int? BatchId
    {
        get => _batchId;
        set => SetProperty(ref _batchId, value);
    }

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public DateTime? CreatedDate
    {
        get => _createdDate;
        set => SetProperty(ref _createdDate, value);
    }

    public bool? IsRead
    {
        get => _isRead;
        set => SetProperty(ref _isRead, value);
    }

    public virtual Batch? Batch { get; set; }
    public virtual Product? Product { get; set; }
}