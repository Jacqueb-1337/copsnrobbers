.class public final Lcom/amazon/device/iap/model/RequestId;
.super Ljava/lang/Object;
.source "RequestId.java"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "Lcom/amazon/device/iap/model/RequestId;",
            ">;"
        }
    .end annotation
.end field

.field private static final ENCODED_ID:Ljava/lang/String; = "encodedId"


# instance fields
.field private final encodedId:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 29
    new-instance v0, Lcom/amazon/device/iap/model/RequestId$1;

    invoke-direct {v0}, Lcom/amazon/device/iap/model/RequestId$1;-><init>()V

    sput-object v0, Lcom/amazon/device/iap/model/RequestId;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method public constructor <init>()V
    .locals 1

    .prologue
    .line 49
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 50
    invoke-static {}, Ljava/util/UUID;->randomUUID()Ljava/util/UUID;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/UUID;->toString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    .line 51
    return-void
.end method

.method private constructor <init>(Landroid/os/Parcel;)V
    .locals 1

    .prologue
    .line 42
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 43
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    .line 44
    return-void
.end method

.method synthetic constructor <init>(Landroid/os/Parcel;Lcom/amazon/device/iap/model/RequestId$1;)V
    .locals 0

    .prologue
    .line 16
    invoke-direct {p0, p1}, Lcom/amazon/device/iap/model/RequestId;-><init>(Landroid/os/Parcel;)V

    return-void
.end method

.method private constructor <init>(Ljava/lang/String;)V
    .locals 1

    .prologue
    .line 53
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 54
    const-string v0, "encodedId"

    invoke-static {p1, v0}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 56
    iput-object p1, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    .line 57
    return-void
.end method

.method public static fromString(Ljava/lang/String;)Lcom/amazon/device/iap/model/RequestId;
    .locals 1

    .prologue
    .line 78
    new-instance v0, Lcom/amazon/device/iap/model/RequestId;

    invoke-direct {v0, p0}, Lcom/amazon/device/iap/model/RequestId;-><init>(Ljava/lang/String;)V

    return-object v0
.end method


# virtual methods
.method public describeContents()I
    .locals 1

    .prologue
    .line 21
    const/4 v0, 0x0

    return v0
.end method

.method public equals(Ljava/lang/Object;)Z
    .locals 2

    .prologue
    .line 91
    if-eqz p1, :cond_0

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    invoke-virtual {p1}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v1

    if-eq v0, v1, :cond_1

    .line 92
    :cond_0
    const/4 v0, 0x0

    .line 95
    :goto_0
    return v0

    :cond_1
    iget-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    check-cast p1, Lcom/amazon/device/iap/model/RequestId;

    iget-object v1, p1, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    goto :goto_0
.end method

.method public hashCode()I
    .locals 1

    .prologue
    .line 100
    .line 102
    iget-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    if-nez v0, :cond_0

    const/4 v0, 0x0

    :goto_0
    add-int/lit8 v0, v0, 0x1f

    .line 103
    return v0

    .line 102
    :cond_0
    iget-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    invoke-virtual {v0}, Ljava/lang/String;->hashCode()I

    move-result v0

    goto :goto_0
.end method

.method public toJSON()Lorg/json/JSONObject;
    .locals 3

    .prologue
    .line 113
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    .line 115
    :try_start_0
    const-string v1, "encodedId"

    iget-object v2, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 120
    :goto_0
    return-object v0

    .line 116
    :catch_0
    move-exception v1

    goto :goto_0
.end method

.method public toString()Ljava/lang/String;
    .locals 1

    .prologue
    .line 66
    iget-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    return-object v0
.end method

.method public writeToParcel(Landroid/os/Parcel;I)V
    .locals 1

    .prologue
    .line 26
    iget-object v0, p0, Lcom/amazon/device/iap/model/RequestId;->encodedId:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 27
    return-void
.end method
