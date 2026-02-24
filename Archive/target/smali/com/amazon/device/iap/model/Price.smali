.class public final Lcom/amazon/device/iap/model/Price;
.super Ljava/lang/Object;
.source "Price.java"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "Lcom/amazon/device/iap/model/Price;",
            ">;"
        }
    .end annotation
.end field

.field private static final CURRENCY:Ljava/lang/String; = "currency"

.field private static final VALUE:Ljava/lang/String; = "value"


# instance fields
.field private final currency:Ljava/util/Currency;

.field private final value:Ljava/math/BigDecimal;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 37
    new-instance v0, Lcom/amazon/device/iap/model/Price$1;

    invoke-direct {v0}, Lcom/amazon/device/iap/model/Price$1;-><init>()V

    sput-object v0, Lcom/amazon/device/iap/model/Price;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method private constructor <init>(Landroid/os/Parcel;)V
    .locals 2

    .prologue
    .line 49
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 50
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Ljava/util/Currency;->getInstance(Ljava/lang/String;)Ljava/util/Currency;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Price;->currency:Ljava/util/Currency;

    .line 51
    new-instance v0, Ljava/math/BigDecimal;

    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/math/BigDecimal;-><init>(Ljava/lang/String;)V

    iput-object v0, p0, Lcom/amazon/device/iap/model/Price;->value:Ljava/math/BigDecimal;

    .line 52
    return-void
.end method

.method synthetic constructor <init>(Landroid/os/Parcel;Lcom/amazon/device/iap/model/Price$1;)V
    .locals 0

    .prologue
    .line 18
    invoke-direct {p0, p1}, Lcom/amazon/device/iap/model/Price;-><init>(Landroid/os/Parcel;)V

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/model/PriceBuilder;)V
    .locals 2

    .prologue
    .line 71
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 72
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PriceBuilder;->getCurrency()Ljava/util/Currency;

    move-result-object v0

    const-string v1, "currency"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 73
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PriceBuilder;->getValue()Ljava/math/BigDecimal;

    move-result-object v0

    const-string v1, "value"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 75
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PriceBuilder;->getCurrency()Ljava/util/Currency;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Price;->currency:Ljava/util/Currency;

    .line 76
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PriceBuilder;->getValue()Ljava/math/BigDecimal;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/Price;->value:Ljava/math/BigDecimal;

    .line 77
    return-void
.end method


# virtual methods
.method public describeContents()I
    .locals 1

    .prologue
    .line 28
    const/4 v0, 0x0

    return v0
.end method

.method public getCurrency()Ljava/util/Currency;
    .locals 1

    .prologue
    .line 59
    iget-object v0, p0, Lcom/amazon/device/iap/model/Price;->currency:Ljava/util/Currency;

    return-object v0
.end method

.method public getValue()Ljava/math/BigDecimal;
    .locals 1

    .prologue
    .line 68
    iget-object v0, p0, Lcom/amazon/device/iap/model/Price;->value:Ljava/math/BigDecimal;

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
    .line 87
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    .line 88
    const-string v1, "currency"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Price;->currency:Ljava/util/Currency;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 89
    const-string v1, "value"

    iget-object v2, p0, Lcom/amazon/device/iap/model/Price;->value:Ljava/math/BigDecimal;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 90
    return-object v0
.end method

.method public toString()Ljava/lang/String;
    .locals 3

    .prologue
    .line 95
    const/4 v0, 0x0

    .line 97
    :try_start_0
    invoke-virtual {p0}, Lcom/amazon/device/iap/model/Price;->toJSON()Lorg/json/JSONObject;

    move-result-object v1

    const/4 v2, 0x4

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->toString(I)Ljava/lang/String;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    .line 101
    :goto_0
    return-object v0

    .line 98
    :catch_0
    move-exception v1

    goto :goto_0
.end method

.method public writeToParcel(Landroid/os/Parcel;I)V
    .locals 1

    .prologue
    .line 33
    iget-object v0, p0, Lcom/amazon/device/iap/model/Price;->currency:Ljava/util/Currency;

    invoke-virtual {v0}, Ljava/util/Currency;->getCurrencyCode()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 34
    iget-object v0, p0, Lcom/amazon/device/iap/model/Price;->value:Ljava/math/BigDecimal;

    invoke-virtual {v0}, Ljava/math/BigDecimal;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 35
    return-void
.end method
