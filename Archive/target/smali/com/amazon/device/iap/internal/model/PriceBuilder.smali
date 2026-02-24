.class public Lcom/amazon/device/iap/internal/model/PriceBuilder;
.super Ljava/lang/Object;
.source "PriceBuilder.java"


# instance fields
.field private currency:Ljava/util/Currency;

.field private value:Ljava/math/BigDecimal;


# direct methods
.method public constructor <init>()V
    .locals 0

    .prologue
    .line 8
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public build()Lcom/amazon/device/iap/model/Price;
    .locals 1

    .prologue
    .line 13
    new-instance v0, Lcom/amazon/device/iap/model/Price;

    invoke-direct {v0, p0}, Lcom/amazon/device/iap/model/Price;-><init>(Lcom/amazon/device/iap/internal/model/PriceBuilder;)V

    return-object v0
.end method

.method public getCurrency()Ljava/util/Currency;
    .locals 1

    .prologue
    .line 27
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/PriceBuilder;->currency:Ljava/util/Currency;

    return-object v0
.end method

.method public getValue()Ljava/math/BigDecimal;
    .locals 1

    .prologue
    .line 31
    iget-object v0, p0, Lcom/amazon/device/iap/internal/model/PriceBuilder;->value:Ljava/math/BigDecimal;

    return-object v0
.end method

.method public setCurrency(Ljava/util/Currency;)Lcom/amazon/device/iap/internal/model/PriceBuilder;
    .locals 0

    .prologue
    .line 17
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/PriceBuilder;->currency:Ljava/util/Currency;

    .line 18
    return-object p0
.end method

.method public setValue(Ljava/math/BigDecimal;)Lcom/amazon/device/iap/internal/model/PriceBuilder;
    .locals 0

    .prologue
    .line 22
    iput-object p1, p0, Lcom/amazon/device/iap/internal/model/PriceBuilder;->value:Ljava/math/BigDecimal;

    .line 23
    return-object p0
.end method
