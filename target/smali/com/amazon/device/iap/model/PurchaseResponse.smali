.class public final Lcom/amazon/device/iap/model/PurchaseResponse;
.super Ljava/lang/Object;
.source "PurchaseResponse.java"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;
    }
.end annotation


# static fields
.field private static final RECEIPT:Ljava/lang/String; = "receipt"

.field private static final REQUEST_ID:Ljava/lang/String; = "requestId"

.field private static final REQUEST_STATUS:Ljava/lang/String; = "requestStatus"

.field private static final TO_STRING_FORMAT:Ljava/lang/String; = "(%s, requestId: \"%s\", purchaseRequestStatus: \"%s\", userId: \"%s\", receipt: %s)"

.field private static final USER_DATA:Ljava/lang/String; = "userData"


# instance fields
.field private final receipt:Lcom/amazon/device/iap/model/Receipt;

.field private final requestId:Lcom/amazon/device/iap/model/RequestId;

.field private final requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

.field private final userData:Lcom/amazon/device/iap/model/UserData;


# direct methods
.method public constructor <init>(Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;)V
    .locals 2

    .prologue
    .line 84
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 86
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getRequestId()Lcom/amazon/device/iap/model/RequestId;

    move-result-object v0

    const-string v1, "requestId"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 87
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getRequestStatus()Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    move-result-object v0

    const-string v1, "requestStatus"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 89
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getRequestStatus()Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    move-result-object v0

    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->SUCCESSFUL:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    if-ne v0, v1, :cond_0

    .line 90
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v0

    const-string v1, "receipt"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 91
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getUserData()Lcom/amazon/device/iap/model/UserData;

    move-result-object v0

    const-string v1, "userData"

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/d;->a(Ljava/lang/Object;Ljava/lang/String;)V

    .line 94
    :cond_0
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getRequestId()Lcom/amazon/device/iap/model/RequestId;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestId:Lcom/amazon/device/iap/model/RequestId;

    .line 95
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getUserData()Lcom/amazon/device/iap/model/UserData;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->userData:Lcom/amazon/device/iap/model/UserData;

    .line 96
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->receipt:Lcom/amazon/device/iap/model/Receipt;

    .line 97
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/model/PurchaseResponseBuilder;->getRequestStatus()Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    .line 98
    return-void
.end method


# virtual methods
.method public getReceipt()Lcom/amazon/device/iap/model/Receipt;
    .locals 1

    .prologue
    .line 131
    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->receipt:Lcom/amazon/device/iap/model/Receipt;

    return-object v0
.end method

.method public getRequestId()Lcom/amazon/device/iap/model/RequestId;
    .locals 1

    .prologue
    .line 108
    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestId:Lcom/amazon/device/iap/model/RequestId;

    return-object v0
.end method

.method public getRequestStatus()Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;
    .locals 1

    .prologue
    .line 140
    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    return-object v0
.end method

.method public getUserData()Lcom/amazon/device/iap/model/UserData;
    .locals 1

    .prologue
    .line 119
    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->userData:Lcom/amazon/device/iap/model/UserData;

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
    .line 161
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 162
    const-string v0, "requestId"

    iget-object v2, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestId:Lcom/amazon/device/iap/model/RequestId;

    invoke-virtual {v1, v0, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 163
    const-string v0, "requestStatus"

    iget-object v2, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1, v0, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 164
    const-string v2, "userData"

    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->userData:Lcom/amazon/device/iap/model/UserData;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->userData:Lcom/amazon/device/iap/model/UserData;

    invoke-virtual {v0}, Lcom/amazon/device/iap/model/UserData;->toJSON()Lorg/json/JSONObject;

    move-result-object v0

    :goto_0
    invoke-virtual {v1, v2, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 165
    const-string v2, "receipt"

    invoke-virtual {p0}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v0

    if-eqz v0, :cond_1

    invoke-virtual {p0}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v0

    invoke-virtual {v0}, Lcom/amazon/device/iap/model/Receipt;->toJSON()Lorg/json/JSONObject;

    move-result-object v0

    :goto_1
    invoke-virtual {v1, v2, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 166
    return-object v1

    .line 164
    :cond_0
    const-string v0, ""

    goto :goto_0

    .line 165
    :cond_1
    const-string v0, ""

    goto :goto_1
.end method

.method public toString()Ljava/lang/String;
    .locals 4

    .prologue
    .line 145
    const-string v1, "(%s, requestId: \"%s\", purchaseRequestStatus: \"%s\", userId: \"%s\", receipt: %s)"

    const/4 v0, 0x5

    new-array v2, v0, [Ljava/lang/Object;

    const/4 v0, 0x0

    invoke-super {p0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v3

    aput-object v3, v2, v0

    const/4 v0, 0x1

    iget-object v3, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestId:Lcom/amazon/device/iap/model/RequestId;

    aput-object v3, v2, v0

    const/4 v3, 0x2

    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->requestStatus:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v0}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->toString()Ljava/lang/String;

    move-result-object v0

    :goto_0
    aput-object v0, v2, v3

    const/4 v0, 0x3

    iget-object v3, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->userData:Lcom/amazon/device/iap/model/UserData;

    aput-object v3, v2, v0

    const/4 v0, 0x4

    iget-object v3, p0, Lcom/amazon/device/iap/model/PurchaseResponse;->receipt:Lcom/amazon/device/iap/model/Receipt;

    aput-object v3, v2, v0

    invoke-static {v1, v2}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    return-object v0

    :cond_0
    const-string v0, "null"

    goto :goto_0
.end method
