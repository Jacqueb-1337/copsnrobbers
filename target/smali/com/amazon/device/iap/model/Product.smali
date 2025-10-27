.class public final Lcom/amazon/device/iap/model/Product;
.super Ljava/lang/Object;
.source "Product.java"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "Lcom/amazon/device/iap/model/Product;",
            ">;"
        }
    .end annotation
.end field

.field private static final DESCRIPTION:Ljava/lang/String; = "description"

.field private static final PRICE:Ljava/lang/String; = "price"

.field private static final PRODUCT_TYPE:Ljava/lang/String; = "productType"

.field public static final SKU:Ljava/lang/String; = "sku"

.field private static final SMALL_ICON_URL:Ljava/lang/String; = "smallIconUrl"

.field private static final TITLE:Ljava/lang/String; = "title"


# instance fields
.field private final description:Ljava/lang/String;

.field private final price:Lcom/amazon/device/iap/model/Price;

.field private final productType:Lcom/amazon/device/iap/model/ProductType;

.field private final sku:Ljava/lang/String;

.field private final smallIconUrl:Ljava/lang/String;

.field private final title:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 49
    new-instance v0, Lcom/amazon/device/iap/model/Product$1;

    invoke-direct {v0}, Lcom/amazon/device/iap/model/Product$1;-><init>()V

    sput-object v0, Lcom/amazon/device/iap/model/Product;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method private constructor <init>(Landroid/os/Parcel;)V
    .locals 1

    .prologue
    .line 61
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 62
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->sku:Ljava/lang/String;

    .line 63
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/amazon/device/iap/model/ProductType;->valueOf(Ljava/lang/String;)Lcom/amazon/device/iap/model/ProductType;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->productType:Lcom/amazon/device/iap/model/ProductType;

    .line 64
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->description:Ljava/lang/String;

    .line 65
    const-class v0, Lcom/amazon/device/iap/model/Price;

    invoke-virtual {v0}, Ljava/lang/Class;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->readParcelable(Ljava/lang/ClassLoader;)Landroid/os/Parcelable;

    move-result-object v0

    check-cast v0, Lcom/amazon/device/iap/model/Price;

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->price:Lcom/amazon/device/iap/model/Price;

    .line 66
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->smallIconUrl:Ljava/lang/String;

    .line 67
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->title:Ljava/lang/String;

    .line 68
    return-void
.end method

.method synthetic constructor <init>(Landroid/os/Parcel;Lcom/amazon/device/iap/model/Product$1;)V
    .locals 0

    .prologue
    .line 16
    invoke-direct {p0, p1}, Lcom/amazon/device/iap/model/Product;-><init>(Landroid/os/Parcel;)V

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/model/ProductBuilder;)V
    .locals 2

    .prologue
    .line 73
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 74
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getSku()Ljava/lang/String;

    move-result-object v0

    const-string v1, "sku"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 75
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getProductType()Lcom/amazon/device/iap/model/ProductType;

    move-result-object v0

    const-string v1, "productType"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 76
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getDescription()Ljava/lang/String;

    move-result-object v0

    const-string v1, "description"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 77
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getTitle()Ljava/lang/String;

    move-result-object v0

    const-string v1, "title"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 78
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getSmallIconUrl()Ljava/lang/String;

    move-result-object v0

    const-string v1, "smallIconUrl"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 80
    sget-object v0, Lcom/amazon/device/iap/model/ProductType;->SUBSCRIPTION:Lcom/amazon/device/iap/model/ProductType;

    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getProductType()Lcom/amazon/device/iap/model/ProductType;

    move-result-object v1

    if-eq v0, v1, :cond_0

    .line 81
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getPrice()Lcom/amazon/device/iap/model/Price;

    move-result-object v0

    const-string v1, "price"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 84
    :cond_0
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getSku()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->sku:Ljava/lang/String;

    .line 85
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getProductType()Lcom/amazon/device/iap/model/ProductType;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->productType:Lcom/amazon/device/iap/model/ProductType;

    .line 86
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getDescription()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->description:Ljava/lang/String;

    .line 87
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getPrice()Lcom/amazon/device/iap/model/Price;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->price:Lcom/amazon/device/iap/model/Price;

    .line 88
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getSmallIconUrl()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->smallIconUrl:Ljava/lang/String;

    .line 89
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/ProductBuilder;->getTitle()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Product;->title:Ljava/lang/String;

    .line 90
    return-void
.end method


# virtual methods
.method public describeContents()I
    .locals 1

    .prologue
    .line 36
    const/4 v0, 0x0

    return v0
.end method

.method public getDescription()Ljava/lang/String;
    .locals 1

    .prologue
    .line 120
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->description:Ljava/lang/String;

    return-object v0
.end method

.method public getPrice()Lcom/amazon/device/iap/model/Price;
    .locals 1

    .prologue
    .line 131
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->price:Lcom/amazon/device/iap/model/Price;

    return-object v0
.end method

.method public getProductType()Lcom/amazon/device/iap/model/ProductType;
    .locals 1

    .prologue
    .line 110
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->productType:Lcom/amazon/device/iap/model/ProductType;

    return-object v0
.end method

.method public getSku()Ljava/lang/String;
    .locals 1

    .prologue
    .line 101
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->sku:Ljava/lang/String;

    return-object v0
.end method

.method public getSmallIconUrl()Ljava/lang/String;
    .locals 1

    .prologue
    .line 140
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->smallIconUrl:Ljava/lang/String;

    return-object v0
.end method

.method public getTitle()Ljava/lang/String;
    .locals 1

    .prologue
    .line 150
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->title:Ljava/lang/String;

    return-object v0
.end method

.method public toJSON()Lorg/json/JSONObject;
    .locals 3
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 159
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    .line 161
    const-string v1, "sku"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->sku:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 162
    const-string v1, "productType"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->productType:Lcom/amazon/device/iap/model/ProductType;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 163
    const-string v1, "description"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->description:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 164
    const-string v1, "price"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->price:Lcom/amazon/device/iap/model/Price;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 165
    const-string v1, "smallIconUrl"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->smallIconUrl:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 166
    const-string v1, "title"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Product;->title:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 168
    return-object v0
.end method

.method public toString()Ljava/lang/String;
    .locals 3

    .prologue
    .line 173
    const/4 v0, 0x0

    .line 175
    :try_start_0
    invoke-virtual {p0}, Lcom/amazon/device/iap/model/Product;->toJSON()Lorg/json/JSONObject;

    move-result-object v1

    const/4 v2, 0x4

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->toString(I)Ljava/lang/String;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    .line 179
    :goto_0
    return-object v0

    .line 176
    :catch_0
    move-exception v1

    goto :goto_0
.end method

.method public writeToParcel(Landroid/os/Parcel;I)V
    .locals 1

    .prologue
    .line 41
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->sku:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 42
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->productType:Lcom/amazon/device/iap/model/ProductType;

    invoke-virtual {v0}, Lcom/amazon/device/iap/model/ProductType;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 43
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->description:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 44
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->price:Lcom/amazon/device/iap/model/Price;

    invoke-virtual {p1, v0, p2}, Landroid/os/Parcel;->writeParcelable(Landroid/os/Parcelable;I)V

    .line 45
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->smallIconUrl:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 46
    iget-object v0, p0, Lcom/amazon/device/iap/model/Product;->title:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 47
    return-void
.end method
