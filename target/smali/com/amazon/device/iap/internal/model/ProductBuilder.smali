.class public Lcom/amazon/device/iap/internal/model/ProductBuilder;
.super Ljava/lang/Object;
.source "ProductBuilder.java"


# instance fields
.field private description:Ljava/lang/String;

.field private price:Lcom/amazon/device/iap/model/Price;

.field private productType:Lcom/amazon/device/iap/model/ProductType;

.field private sku:Ljava/lang/String;

.field private smallIconUrl:Ljava/lang/String;

.field private title:Ljava/lang/String;


# direct methods
.method public constructor <init>()V
    .locals 0

    .prologue
    .line 7
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public build()Lcom/amazon/device/iap/model/Product;
    .locals 1

    .prologue
    .line 16
    new-instance v0, Lcom/amazon/device/iap/model/Product;

    invoke-direct {v0, p0}, Lcom/amazon/device/iap/model/Product;-><init>(Lcom/amazon/device/iap/internal/model/ProductBuilder;)V

    return-object v0
.end method

.method public getDescription()Ljava/lang/String;
    .locals 1

    .prologue
    .line 58
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->description:Ljava/lang/String;

    return-object v0
.end method

.method public getPrice()Lcom/amazon/device/iap/model/Price;
    .locals 1

    .prologue
    .line 62
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->price:Lcom/amazon/device/iap/model/Price;

    return-object v0
.end method

.method public getProductType()Lcom/amazon/device/iap/model/ProductType;
    .locals 1

    .prologue
    .line 54
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->productType:Lcom/amazon/device/iap/model/ProductType;

    return-object v0
.end method

.method public getSku()Ljava/lang/String;
    .locals 1

    .prologue
    .line 50
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->sku:Ljava/lang/String;

    return-object v0
.end method

.method public getSmallIconUrl()Ljava/lang/String;
    .locals 1

    .prologue
    .line 66
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->smallIconUrl:Ljava/lang/String;

    return-object v0
.end method

.method public getTitle()Ljava/lang/String;
    .locals 1

    .prologue
    .line 70
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->title:Ljava/lang/String;

    return-object v0
.end method

.method public setDescription(Ljava/lang/String;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 30
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->description:Ljava/lang/String;

    .line 31
    return-object p0
.end method

.method public setPrice(Lcom/amazon/device/iap/model/Price;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 35
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->price:Lcom/amazon/device/iap/model/Price;

    .line 36
    return-object p0
.end method

.method public setProductType(Lcom/amazon/device/iap/model/ProductType;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 25
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->productType:Lcom/amazon/device/iap/model/ProductType;

    .line 26
    return-object p0
.end method

.method public setSku(Ljava/lang/String;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 20
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->sku:Ljava/lang/String;

    .line 21
    return-object p0
.end method

.method public setSmallIconUrl(Ljava/lang/String;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 40
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->smallIconUrl:Ljava/lang/String;

    .line 41
    return-object p0
.end method

.method public setTitle(Ljava/lang/String;)Lcom/amazon/device/iap/internal/model/ProductBuilder;
    .locals 0

    .prologue
    .line 45
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/ProductBuilder;->title:Ljava/lang/String;

    .line 46
    return-object p0
.end method
