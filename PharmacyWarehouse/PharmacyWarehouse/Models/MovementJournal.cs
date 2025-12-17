using System;
using System.Collections.Generic;

namespace PharmacyWarehouse.Models;

public partial class MovementJournal : ObservableObject
{
    private int _id;
    private DateTime? _operationDate;
    private string? _operationType;
    private int? _documentId;
    private int? _batchId;
    private int? _quantityChange;
    private int? _remainingQuantity;
    private string? _userName;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public DateTime? OperationDate
    {
        get => _operationDate;
        set => SetProperty(ref _operationDate, value);
    }

    public string? OperationType
    {
        get => _operationType;
        set => SetProperty(ref _operationType, value);
    }

    public int? DocumentId
    {
        get => _documentId;
        set => SetProperty(ref _documentId, value);
    }

    public int? BatchId
    {
        get => _batchId;
        set => SetProperty(ref _batchId, value);
    }

    public int? QuantityChange
    {
        get => _quantityChange;
        set => SetProperty(ref _quantityChange, value);
    }

    public int? RemainingQuantity
    {
        get => _remainingQuantity;
        set => SetProperty(ref _remainingQuantity, value);
    }

    public string? UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public virtual Batch? Batch { get; set; }
    public virtual Document? Document { get; set; }
}