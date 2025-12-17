using System;
using System.Collections.Generic;

namespace PharmacyWarehouse.Models;

public partial class WriteOff : ObservableObject
{
    private int _id;
    private int _documentId;
    private string _reason = null!;
    private string? _comment;

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

    public string Reason
    {
        get => _reason;
        set => SetProperty(ref _reason, value);
    }

    public string? Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public virtual Document Document { get; set; } = null!;
}