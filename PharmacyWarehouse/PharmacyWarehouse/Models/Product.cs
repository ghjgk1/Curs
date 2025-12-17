using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace PharmacyWarehouse.Models;

public partial class Product : ObservableObject
{
    private int _id;
    private string _name = null!;
    private int? _categoryId;
    private string? _releaseForm;
    private string? _manufacturer;
    private bool? _requiresPrescription;
    private string? _unitOfMeasure;
    private int? _minRemainder;
    private string? _description;

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

    public int? CategoryId
    {
        get => _categoryId;
        set => SetProperty(ref _categoryId, value);
    }

    public string? ReleaseForm
    {
        get => _releaseForm;
        set => SetProperty(ref _releaseForm, value);
    }

    public string? Manufacturer
    {
        get => _manufacturer;
        set => SetProperty(ref _manufacturer, value);
    }

    public bool? RequiresPrescription
    {
        get => _requiresPrescription;
        set => SetProperty(ref _requiresPrescription, value);
    }

    public string? UnitOfMeasure
    {
        get => _unitOfMeasure;
        set => SetProperty(ref _unitOfMeasure, value);
    }

    public int? MinRemainder
    {
        get => _minRemainder;
        set => SetProperty(ref _minRemainder, value);
    }
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public virtual ICollection<Batch> Batches { get; set; } = new ObservableCollection<Batch>();
    public virtual Category? Category { get; set; }
    public virtual ICollection<InventoryResult> InventoryResults { get; set; } = new ObservableCollection<InventoryResult>();
    public virtual ICollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();

    [NotMapped]
    public string StatusText
    {
        get
        {
            if (CurrentStock <= 0) return "Нет в наличии";
            if (CurrentStock <= MinRemainder) return "Мало";
            return "В наличии";
        }
    }

    [NotMapped]
    public Brush StatusBrush
    {
        get
        {
            if (CurrentStock <= 0) return Brushes.Red;
            if (CurrentStock <= MinRemainder) return Brushes.Orange;
            return Brushes.Green;
        }
    }

    [NotMapped]
    public bool IsLowStock => CurrentStock > 0 && CurrentStock <= MinRemainder;

    [NotMapped]
    public bool IsOutOfStock => CurrentStock <= 0;

    [NotMapped]
    public decimal CurrentStock => Batches?.Where(b => !b.IsExpired).Sum(b => b.Quantity) ?? 0;

    [NotMapped]
    public bool HasExpiredBatches => Batches?.Any(b => b.IsExpired) ?? false;

    [NotMapped]
    public bool HasExpiringBatches => Batches?.Any(b => b.IsExpiringSoon) ?? false;

    [NotMapped]
    public decimal TotalValue => CurrentStock ;
}