.class public final Lcom/amazon/device/iap/model/UserData;
.super Ljava/lang/Object;
.source "UserData.java"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "Lcom/amazon/device/iap/model/UserData;",
            ">;"
        }
    .end annotation
.end field

.field private static final MARKETPLACE:Ljava/lang/String; = "marketplace"

.field private static final USER_ID:Ljava/lang/String; = "userId"


# instance fields
.field private final marketplace:Ljava/lang/String;

.field private final userId:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 31
    new-instance v0, Lcom/amazon/device/iap/model/UserData$1;

    invoke-direct {v0}, Lcom/amazon/device/iap/model/UserData$1;-><init>()V

    sput-object v0, Lcom/amazon/device/iap/model/UserData;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method private constructor <init>(Landroid/os/Parcel;)V
    .locals 1

    .prologue
    .line 43
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 44
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/UserData;->userId:Ljava/lang/String;

    .line 45
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/UserData;->marketplace:Ljava/lang/String;

    .line 46
    return-void
.end method

.method synthetic constructor <init>(Landroid/os/Parcel;Lcom/amazon/device/iap/model/UserData$1;)V
    .locals 0

    .prologue
    .line 14
    invoke-direct {p0, p1}, Lcom/amazon/device/iap/model/UserData;-><init>(Landroid/os/Parcel;)V

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/model/UserDataBuilder;)V
    .locals 1

    .prologue
    .line 100
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 101
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/UserDataBuilder;->getUserId()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/UserData;->userId:Ljava/lang/String;

    .line 102
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/UserDataBuilder;->getMarketplace()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/UserData;->marketplace:Ljava/lang/String;

    .line 103
    return-void
.end method


# virtual methods
.method public describeContents()I
    .locals 1

    .prologue
    .line 23
    const/4 v0, 0x0

    return v0
.end method

.method public getMarketplace()Ljava/lang/String;
    .locals 1

    .prologue
    .line 67
    iget-object v0, p0, Lcom/amazon/device/iap/model/UserData;->marketplace:Ljava/lang/String;

    return-object v0
.end method

.method public getUserId()Ljava/lang/String;
    .locals 1

    .prologue
    .line 55
    iget-object v0, p0, Lcom/amazon/device/iap/model/UserData;->userId:Ljava/lang/String;

    return-object v0
.end method

.method public toJSON()Lorg/json/JSONObject;
    .locals 3

    .prologue
    .line 77
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    .line 80
    :try_start_0
    const-string v1, "userId"

    iget-object v2, p0, Lcom/amazon/device/iap/model/UserData;->userId:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 81
    const-string v1, "marketplace"

    iget-object v2, p0, Lcom/amazon/device/iap/model/UserData;->marketplace:Ljava/lang/String;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 86
    :goto_0
    return-object v0

    .line 82
    :catch_0
    move-exception v1

    goto :goto_0
.end method

.method public toString()Ljava/lang/String;
    .locals 3

    .prologue
    .line 91
    const/4 v0, 0x0

    .line 93
    :try_start_0
    invoke-virtual {p0}, Lcom/amazon/device/iap/model/UserData;->toJSON()Lorg/json/JSONObject;

    move-result-object v1

    const/4 v2, 0x4

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->toString(I)Ljava/lang/String;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    .line 97
    :goto_0
    return-object v0

    .line 94
    :catch_0
    move-exception v1

    goto :goto_0
.end method

.method public writeToParcel(Landroid/os/Parcel;I)V
    .locals 3

    .prologue
    .line 28
    const/4 v0, 0x2

    new-array v0, v0, [Ljava/lang/String;

    const/4 v1, 0x0

    iget-object v2, p0, Lcom/amazon/device/iap/model/UserData;->userId:Ljava/lang/String;

    aput-object v2, v0, v1

    const/4 v1, 0x1

    iget-object v2, p0, Lcom/amazon/device/iap/model/UserData;->marketplace:Ljava/lang/String;

    aput-object v2, v0, v1

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeStringArray([Ljava/lang/String;)V

    .line 29
    return-void
.end method
