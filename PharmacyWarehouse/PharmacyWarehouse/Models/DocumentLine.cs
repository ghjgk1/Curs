using System;
using System.Collections.Generic;

namespace PharmacyWarehouse.Models;

public partial class DocumentLine : ObservableObject
{
    private int _id;
    private int _documentId;
    private int _batchId;
    private int _quantity;
    private decimal _price;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int DocumentId
    {
        get => _documentId;
        set => SetProperty(ref _documentId, value);
    }

    public int BatchId
    {
        get => _batchId;
        set => SetProperty(ref _batchId, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public virtual Batch Batch { get; set; } = null!;
    public virtual Document Document { get; set; } = null!;
}
