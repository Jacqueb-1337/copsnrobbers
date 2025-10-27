.class public Lcom/outlinegames/unibillAmazon/Unibill;
.super Ljava/lang/Object;
.source "Unibill.java"

# interfaces
.implements Lcom/amazon/device/iap/PurchasingListener;


# static fields
.field private static synthetic $SWITCH_TABLE$com$amazon$device$iap$model$PurchaseResponse$RequestStatus:[I = null

.field private static final GAMEOBJECT_NAME:Ljava/lang/String; = "AmazonAppStoreCallbackMonoBehaviour"

.field private static final LOG_PREFIX:Ljava/lang/String; = "UnibillAmazonPlugin"

.field private static PRODUCT_UNDER_PURCHASE:Ljava/lang/String;

.field private static instance:Lcom/outlinegames/unibillAmazon/Unibill;

.field private static userId:Ljava/lang/String;


# instance fields
.field private itemGroups:Ljava/util/Queue;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Queue",
            "<",
            "Ljava/util/Set",
            "<",
            "Ljava/lang/String;",
            ">;>;"
        }
    .end annotation
.end field

.field private items:Ljava/util/ArrayList;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/ArrayList",
            "<",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation
.end field

.field private jsonResponse:Lorg/json/JSONObject;

.field products:Lorg/json/JSONObject;

.field private restorePending:Z

.field private restored:Lorg/json/JSONArray;

.field private revoked:Lorg/json/JSONArray;


# direct methods
.method static synthetic $SWITCH_TABLE$com$amazon$device$iap$model$PurchaseResponse$RequestStatus()[I
    .locals 3

    .prologue
    .line 30
    sget-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->$SWITCH_TABLE$com$amazon$device$iap$model$PurchaseResponse$RequestStatus:[I

    if-eqz v0, :cond_0

    :goto_0
    return-object v0

    :cond_0
    invoke-static {}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->values()[Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    move-result-object v0

    array-length v0, v0

    new-array v0, v0, [I

    :try_start_0
    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ALREADY_PURCHASED:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v1

    const/4 v2, 0x4

    aput v2, v0, v1
    :try_end_0
    .catch Ljava/lang/NoSuchFieldError; {:try_start_0 .. :try_end_0} :catch_4

    :goto_1
    :try_start_1
    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->FAILED:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v1

    const/4 v2, 0x2

    aput v2, v0, v1
    :try_end_1
    .catch Ljava/lang/NoSuchFieldError; {:try_start_1 .. :try_end_1} :catch_3

    :goto_2
    :try_start_2
    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->INVALID_SKU:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v1

    const/4 v2, 0x3

    aput v2, v0, v1
    :try_end_2
    .catch Ljava/lang/NoSuchFieldError; {:try_start_2 .. :try_end_2} :catch_2

    :goto_3
    :try_start_3
    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->NOT_SUPPORTED:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v1

    const/4 v2, 0x5

    aput v2, v0, v1
    :try_end_3
    .catch Ljava/lang/NoSuchFieldError; {:try_start_3 .. :try_end_3} :catch_1

    :goto_4
    :try_start_4
    sget-object v1, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->SUCCESSFUL:Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v1

    const/4 v2, 0x1

    aput v2, v0, v1
    :try_end_4
    .catch Ljava/lang/NoSuchFieldError; {:try_start_4 .. :try_end_4} :catch_0

    :goto_5
    sput-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->$SWITCH_TABLE$com$amazon$device$iap$model$PurchaseResponse$RequestStatus:[I

    goto :goto_0

    :catch_0
    move-exception v1

    goto :goto_5

    :catch_1
    move-exception v1

    goto :goto_4

    :catch_2
    move-exception v1

    goto :goto_3

    :catch_3
    move-exception v1

    goto :goto_2

    :catch_4
    move-exception v1

    goto :goto_1
.end method

.method public constructor <init>()V
    .locals 1

    .prologue
    .line 30
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 68
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    iput-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->products:Lorg/json/JSONObject;

    .line 30
    return-void
.end method

.method private SendUnityPurchaseSuccessMessage(Lcom/amazon/device/iap/model/Receipt;)V
    .locals 6
    .param p1, "receipt"    # Lcom/amazon/device/iap/model/Receipt;

    .prologue
    .line 157
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 160
    .local v1, "jsonResponse":Lorg/json/JSONObject;
    if-nez p1, :cond_0

    :try_start_0
    sget-object v2, Lcom/outlinegames/unibillAmazon/Unibill;->PRODUCT_UNDER_PURCHASE:Ljava/lang/String;

    .line 161
    .local v2, "sku":Ljava/lang/String;
    :goto_0
    const-string v3, "productId"

    invoke-virtual {v1, v3, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 162
    const-string v3, "purchaseToken"

    invoke-direct {p0, p1}, Lcom/outlinegames/unibillAmazon/Unibill;->encodeReceipt(Lcom/amazon/device/iap/model/Receipt;)Lorg/json/JSONObject;

    move-result-object v4

    invoke-virtual {v4}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v1, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 166
    .end local v2    # "sku":Ljava/lang/String;
    :goto_1
    const-string v3, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v4, "onPurchaseSucceeded"

    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-static {v3, v4, v5}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 167
    return-void

    .line 160
    :cond_0
    :try_start_1
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/Receipt;->getSku()Ljava/lang/String;
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_0

    move-result-object v2

    goto :goto_0

    .line 163
    :catch_0
    move-exception v0

    .line 164
    .local v0, "j":Lorg/json/JSONException;
    const-string v3, "UnibillAmazonPlugin"

    invoke-virtual {v0}, Lorg/json/JSONException;->getMessage()Ljava/lang/String;

    move-result-object v4

    invoke-static {v3, v4}, Landroid/util/Log;->e(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_1
.end method

.method private encodeReceipt(Lcom/amazon/device/iap/model/Receipt;)Lorg/json/JSONObject;
    .locals 4
    .param p1, "receipt"    # Lcom/amazon/device/iap/model/Receipt;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 148
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    .line 149
    .local v0, "receiptBundle":Lorg/json/JSONObject;
    if-nez p1, :cond_0

    const-string v1, ""

    .line 150
    .local v1, "receiptId":Ljava/lang/String;
    :goto_0
    const-string v2, "receiptId"

    invoke-virtual {v0, v2, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 151
    const-string v2, "userId"

    sget-object v3, Lcom/outlinegames/unibillAmazon/Unibill;->userId:Ljava/lang/String;

    invoke-virtual {v0, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 152
    const-string v2, "isSandbox"

    sget-boolean v3, Lcom/amazon/device/iap/PurchasingService;->IS_SANDBOX_MODE:Z

    invoke-virtual {v0, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Z)Lorg/json/JSONObject;

    .line 153
    return-object v0

    .line 149
    .end local v1    # "receiptId":Ljava/lang/String;
    :cond_0
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/Receipt;->getReceiptId()Ljava/lang/String;

    move-result-object v1

    goto :goto_0
.end method

.method private initiatePurchaseUpdate(Z)V
    .locals 3
    .param p1, "restoreAll"    # Z
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 174
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    iput-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->jsonResponse:Lorg/json/JSONObject;

    .line 175
    new-instance v0, Lorg/json/JSONArray;

    invoke-direct {v0}, Lorg/json/JSONArray;-><init>()V

    iput-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->revoked:Lorg/json/JSONArray;

    .line 176
    new-instance v0, Lorg/json/JSONArray;

    invoke-direct {v0}, Lorg/json/JSONArray;-><init>()V

    iput-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restored:Lorg/json/JSONArray;

    .line 177
    iget-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->jsonResponse:Lorg/json/JSONObject;

    const-string v1, "revoked"

    iget-object v2, p0, Lcom/outlinegames/unibillAmazon/Unibill;->revoked:Lorg/json/JSONArray;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 178
    iget-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->jsonResponse:Lorg/json/JSONObject;

    const-string v1, "restored"

    iget-object v2, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restored:Lorg/json/JSONArray;

    invoke-virtual {v0, v1, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 179
    invoke-static {p1}, Lcom/amazon/device/iap/PurchasingService;->getPurchaseUpdates(Z)Lcom/amazon/device/iap/model/RequestId;

    .line 180
    return-void
.end method

.method public static instance()Lcom/outlinegames/unibillAmazon/Unibill;
    .locals 1

    .prologue
    .line 40
    sget-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->instance:Lcom/outlinegames/unibillAmazon/Unibill;

    if-nez v0, :cond_0

    .line 41
    new-instance v0, Lcom/outlinegames/unibillAmazon/Unibill;

    invoke-direct {v0}, Lcom/outlinegames/unibillAmazon/Unibill;-><init>()V

    sput-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->instance:Lcom/outlinegames/unibillAmazon/Unibill;

    .line 43
    :cond_0
    sget-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->instance:Lcom/outlinegames/unibillAmazon/Unibill;

    return-object v0
.end method


# virtual methods
.method public initiateItemDataRequest([Ljava/lang/String;)V
    .locals 2
    .param p1, "products"    # [Ljava/lang/String;

    .prologue
    .line 48
    const-string v0, "UnibillAmazonPlugin"

    const-string v1, "initiateItemDataRequest"

    invoke-static {v0, v1}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 49
    new-instance v0, Ljava/util/ArrayList;

    invoke-static {p1}, Ljava/util/Arrays;->asList([Ljava/lang/Object;)Ljava/util/List;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/util/ArrayList;-><init>(Ljava/util/Collection;)V

    iput-object v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->items:Ljava/util/ArrayList;

    .line 50
    sget-object v0, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    invoke-static {v0, p0}, Lcom/amazon/device/iap/PurchasingService;->registerListener(Landroid/content/Context;Lcom/amazon/device/iap/PurchasingListener;)V

    .line 51
    invoke-static {}, Lcom/amazon/device/iap/PurchasingService;->getUserData()Lcom/amazon/device/iap/model/RequestId;

    .line 52
    return-void
.end method

.method public initiatePurchaseRequest(Ljava/lang/String;)V
    .locals 2
    .param p1, "productId"    # Ljava/lang/String;

    .prologue
    .line 55
    const-string v0, "UnibillAmazonPlugin"

    const-string v1, "initiatePurchaseRequest"

    invoke-static {v0, v1}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 56
    sput-object p1, Lcom/outlinegames/unibillAmazon/Unibill;->PRODUCT_UNDER_PURCHASE:Ljava/lang/String;

    .line 57
    invoke-static {p1}, Lcom/amazon/device/iap/PurchasingService;->purchase(Ljava/lang/String;)Lcom/amazon/device/iap/model/RequestId;

    .line 58
    return-void
.end method

.method public onProductDataResponse(Lcom/amazon/device/iap/model/ProductDataResponse;)V
    .locals 12
    .param p1, "response"    # Lcom/amazon/device/iap/model/ProductDataResponse;

    .prologue
    .line 71
    const-string v7, "UnibillAmazonPlugin"

    const-string v8, "onItemDataResponse"

    invoke-static {v7, v8}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 73
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/ProductDataResponse;->getRequestStatus()Lcom/amazon/device/iap/model/ProductDataResponse$RequestStatus;

    move-result-object v7

    sget-object v8, Lcom/amazon/device/iap/model/ProductDataResponse$RequestStatus;->SUCCESSFUL:Lcom/amazon/device/iap/model/ProductDataResponse$RequestStatus;

    if-eq v7, v8, :cond_0

    .line 74
    const-string v7, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v8, "onGetItemDataFailed"

    const-string v9, ""

    invoke-static {v7, v8, v9}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 114
    :goto_0
    return-void

    .line 78
    :cond_0
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/ProductDataResponse;->getProductData()Ljava/util/Map;

    move-result-object v7

    invoke-interface {v7}, Ljava/util/Map;->entrySet()Ljava/util/Set;

    move-result-object v7

    invoke-interface {v7}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v8

    :goto_1
    invoke-interface {v8}, Ljava/util/Iterator;->hasNext()Z

    move-result v7

    if-nez v7, :cond_1

    .line 99
    iget-object v7, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    invoke-interface {v7}, Ljava/util/Queue;->isEmpty()Z

    move-result v7

    if-eqz v7, :cond_2

    .line 100
    new-instance v3, Lorg/json/JSONObject;

    invoke-direct {v3}, Lorg/json/JSONObject;-><init>()V

    .line 102
    .local v3, "jsonResponse":Lorg/json/JSONObject;
    :try_start_0
    const-string v7, "userId"

    sget-object v8, Lcom/outlinegames/unibillAmazon/Unibill;->userId:Ljava/lang/String;

    invoke-virtual {v3, v7, v8}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 103
    const-string v7, "products"

    iget-object v8, p0, Lcom/outlinegames/unibillAmazon/Unibill;->products:Lorg/json/JSONObject;

    invoke-virtual {v3, v7, v8}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 104
    const-string v7, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v8, "onProductListReceived"

    invoke-virtual {v3}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v9

    invoke-static {v7, v8, v9}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 105
    new-instance v7, Lorg/json/JSONObject;

    invoke-direct {v7}, Lorg/json/JSONObject;-><init>()V

    iput-object v7, p0, Lcom/outlinegames/unibillAmazon/Unibill;->products:Lorg/json/JSONObject;

    .line 107
    const/4 v7, 0x1

    invoke-direct {p0, v7}, Lcom/outlinegames/unibillAmazon/Unibill;->initiatePurchaseUpdate(Z)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    .line 108
    :catch_0
    move-exception v2

    .line 109
    .local v2, "j":Lorg/json/JSONException;
    const-string v7, "UnibillAmazonPlugin"

    invoke-virtual {v2}, Lorg/json/JSONException;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-static {v7, v8}, Landroid/util/Log;->e(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_0

    .line 78
    .end local v2    # "j":Lorg/json/JSONException;
    .end local v3    # "jsonResponse":Lorg/json/JSONObject;
    :cond_1
    invoke-interface {v8}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    .line 79
    .local v0, "entry":Ljava/util/Map$Entry;, "Ljava/util/Map$Entry<Ljava/lang/String;Lcom/amazon/device/iap/model/Product;>;"
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 81
    .local v1, "item":Lorg/json/JSONObject;
    :try_start_1
    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v6

    check-cast v6, Lcom/amazon/device/iap/model/Product;

    .line 82
    .local v6, "product":Lcom/amazon/device/iap/model/Product;
    const-string v7, "localizedTitle"

    invoke-virtual {v6}, Lcom/amazon/device/iap/model/Product;->getTitle()Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v1, v7, v9}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 83
    const-string v7, "localizedDescription"

    invoke-virtual {v6}, Lcom/amazon/device/iap/model/Product;->getDescription()Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v1, v7, v9}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 85
    invoke-virtual {v6}, Lcom/amazon/device/iap/model/Product;->getPrice()Lcom/amazon/device/iap/model/Price;

    move-result-object v5

    .line 86
    .local v5, "price":Lcom/amazon/device/iap/model/Price;
    invoke-static {}, Ljava/text/NumberFormat;->getCurrencyInstance()Ljava/text/NumberFormat;

    move-result-object v4

    .line 87
    .local v4, "numberFormat":Ljava/text/NumberFormat;
    invoke-virtual {v5}, Lcom/amazon/device/iap/model/Price;->getCurrency()Ljava/util/Currency;

    move-result-object v7

    invoke-virtual {v4, v7}, Ljava/text/NumberFormat;->setCurrency(Ljava/util/Currency;)V

    .line 88
    const-string v7, "price"

    invoke-virtual {v5}, Lcom/amazon/device/iap/model/Price;->getValue()Ljava/math/BigDecimal;

    move-result-object v9

    invoke-virtual {v4, v9}, Ljava/text/NumberFormat;->format(Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v1, v7, v9}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 90
    const-string v9, "isoCurrencyCode"

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v7

    check-cast v7, Lcom/amazon/device/iap/model/Product;

    invoke-virtual {v7}, Lcom/amazon/device/iap/model/Product;->getPrice()Lcom/amazon/device/iap/model/Price;

    move-result-object v7

    invoke-virtual {v7}, Lcom/amazon/device/iap/model/Price;->getCurrency()Ljava/util/Currency;

    move-result-object v7

    invoke-virtual {v7}, Ljava/util/Currency;->getCurrencyCode()Ljava/lang/String;

    move-result-object v7

    invoke-virtual {v1, v9, v7}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 91
    const-string v9, "priceDecimal"

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v7

    check-cast v7, Lcom/amazon/device/iap/model/Product;

    invoke-virtual {v7}, Lcom/amazon/device/iap/model/Product;->getPrice()Lcom/amazon/device/iap/model/Price;

    move-result-object v7

    invoke-virtual {v7}, Lcom/amazon/device/iap/model/Price;->getValue()Ljava/math/BigDecimal;

    move-result-object v7

    invoke-virtual {v7}, Ljava/math/BigDecimal;->doubleValue()D

    move-result-wide v10

    invoke-virtual {v1, v9, v10, v11}, Lorg/json/JSONObject;->put(Ljava/lang/String;D)Lorg/json/JSONObject;

    .line 93
    iget-object v9, p0, Lcom/outlinegames/unibillAmazon/Unibill;->products:Lorg/json/JSONObject;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v7

    check-cast v7, Ljava/lang/String;

    invoke-virtual {v9, v7, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_1

    goto/16 :goto_1

    .line 94
    .end local v4    # "numberFormat":Ljava/text/NumberFormat;
    .end local v5    # "price":Lcom/amazon/device/iap/model/Price;
    .end local v6    # "product":Lcom/amazon/device/iap/model/Product;
    :catch_1
    move-exception v2

    .line 95
    .restart local v2    # "j":Lorg/json/JSONException;
    const-string v7, "Unibill"

    const-string v9, "JSON Exception composing product response"

    invoke-static {v7, v9, v2}, Landroid/util/Log;->e(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Throwable;)I

    goto/16 :goto_1

    .line 112
    .end local v0    # "entry":Ljava/util/Map$Entry;, "Ljava/util/Map$Entry<Ljava/lang/String;Lcom/amazon/device/iap/model/Product;>;"
    .end local v1    # "item":Lorg/json/JSONObject;
    .end local v2    # "j":Lorg/json/JSONException;
    :cond_2
    iget-object v7, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    invoke-interface {v7}, Ljava/util/Queue;->remove()Ljava/lang/Object;

    move-result-object v7

    check-cast v7, Ljava/util/Set;

    invoke-static {v7}, Lcom/amazon/device/iap/PurchasingService;->getProductData(Ljava/util/Set;)Lcom/amazon/device/iap/model/RequestId;

    goto/16 :goto_0
.end method

.method public onPurchaseResponse(Lcom/amazon/device/iap/model/PurchaseResponse;)V
    .locals 4
    .param p1, "purchaseResponse"    # Lcom/amazon/device/iap/model/PurchaseResponse;

    .prologue
    .line 118
    const-string v1, "UnibillAmazonPlugin"

    const-string v2, "onPurchaseResponse"

    invoke-static {v1, v2}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 119
    sget-object v0, Lcom/outlinegames/unibillAmazon/Unibill;->PRODUCT_UNDER_PURCHASE:Ljava/lang/String;

    .line 121
    .local v0, "sku":Ljava/lang/String;
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v1

    if-eqz v1, :cond_0

    .line 122
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v1

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/Receipt;->getSku()Ljava/lang/String;

    move-result-object v0

    .line 125
    :cond_0
    invoke-static {}, Lcom/outlinegames/unibillAmazon/Unibill;->$SWITCH_TABLE$com$amazon$device$iap$model$PurchaseResponse$RequestStatus()[I

    move-result-object v1

    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getRequestStatus()Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;

    move-result-object v2

    invoke-virtual {v2}, Lcom/amazon/device/iap/model/PurchaseResponse$RequestStatus;->ordinal()I

    move-result v2

    aget v1, v1, v2

    packed-switch v1, :pswitch_data_0

    .line 145
    :goto_0
    return-void

    .line 128
    :pswitch_0
    const-string v1, "UnibillAmazonPlugin"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Failed to purchase:"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 129
    const-string v1, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v2, "onPurchaseFailed"

    invoke-static {v1, v2, v0}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 132
    :pswitch_1
    const-string v1, "UnibillAmazonPlugin"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Already entitled to:"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 133
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v1

    invoke-direct {p0, v1}, Lcom/outlinegames/unibillAmazon/Unibill;->SendUnityPurchaseSuccessMessage(Lcom/amazon/device/iap/model/Receipt;)V

    goto :goto_0

    .line 136
    :pswitch_2
    const-string v1, "UnibillAmazonPlugin"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Invalid SKU:"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 137
    const-string v1, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v2, "onPurchaseFailed"

    invoke-static {v1, v2, v0}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 140
    :pswitch_3
    const-string v1, "UnibillAmazonPlugin"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Successfully purchased:"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 141
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v1

    invoke-virtual {v1}, Lcom/amazon/device/iap/model/Receipt;->getReceiptId()Ljava/lang/String;

    move-result-object v1

    sget-object v2, Lcom/amazon/device/iap/model/FulfillmentResult;->FULFILLED:Lcom/amazon/device/iap/model/FulfillmentResult;

    invoke-static {v1, v2}, Lcom/amazon/device/iap/PurchasingService;->notifyFulfillment(Ljava/lang/String;Lcom/amazon/device/iap/model/FulfillmentResult;)V

    .line 142
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseResponse;->getReceipt()Lcom/amazon/device/iap/model/Receipt;

    move-result-object v1

    invoke-direct {p0, v1}, Lcom/outlinegames/unibillAmazon/Unibill;->SendUnityPurchaseSuccessMessage(Lcom/amazon/device/iap/model/Receipt;)V

    goto :goto_0

    .line 125
    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_3
        :pswitch_0
        :pswitch_2
        :pswitch_1
        :pswitch_0
    .end packed-switch
.end method

.method public onPurchaseUpdatesResponse(Lcom/amazon/device/iap/model/PurchaseUpdatesResponse;)V
    .locals 7
    .param p1, "response"    # Lcom/amazon/device/iap/model/PurchaseUpdatesResponse;

    .prologue
    const/4 v6, 0x0

    .line 184
    const-string v3, "UnibillAmazonPlugin"

    const-string v4, "onPurchaseUpdatesResponse"

    invoke-static {v3, v4}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 185
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseUpdatesResponse;->getRequestStatus()Lcom/amazon/device/iap/model/PurchaseUpdatesResponse$RequestStatus;

    move-result-object v3

    sget-object v4, Lcom/amazon/device/iap/model/PurchaseUpdatesResponse$RequestStatus;->SUCCESSFUL:Lcom/amazon/device/iap/model/PurchaseUpdatesResponse$RequestStatus;

    if-eq v3, v4, :cond_1

    .line 186
    const-string v3, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v4, "onPurchaseUpdateFailed"

    const-string v5, ""

    invoke-static {v3, v4, v5}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 187
    iget-boolean v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restorePending:Z

    if-eqz v3, :cond_0

    .line 188
    const-string v3, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v4, "onTransactionsRestored"

    const-string v5, "false"

    invoke-static {v3, v4, v5}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 189
    iput-boolean v6, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restorePending:Z

    .line 217
    :cond_0
    :goto_0
    return-void

    .line 195
    :cond_1
    :try_start_0
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseUpdatesResponse;->getReceipts()Ljava/util/List;

    move-result-object v3

    invoke-interface {v3}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v3

    :cond_2
    :goto_1
    invoke-interface {v3}, Ljava/util/Iterator;->hasNext()Z
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    move-result v4

    if-nez v4, :cond_3

    .line 208
    :goto_2
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/PurchaseUpdatesResponse;->hasMore()Z

    move-result v3

    if-eqz v3, :cond_4

    .line 209
    invoke-static {v6}, Lcom/amazon/device/iap/PurchasingService;->getPurchaseUpdates(Z)Lcom/amazon/device/iap/model/RequestId;

    goto :goto_0

    .line 195
    :cond_3
    :try_start_1
    invoke-interface {v3}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v2

    check-cast v2, Lcom/amazon/device/iap/model/Receipt;

    .line 196
    .local v2, "receipt":Lcom/amazon/device/iap/model/Receipt;
    invoke-virtual {v2}, Lcom/amazon/device/iap/model/Receipt;->isCanceled()Z

    move-result v4

    if-nez v4, :cond_2

    .line 197
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 198
    .local v1, "purchase":Lorg/json/JSONObject;
    iget-object v4, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restored:Lorg/json/JSONArray;

    invoke-virtual {v4, v1}, Lorg/json/JSONArray;->put(Ljava/lang/Object;)Lorg/json/JSONArray;

    .line 199
    const-string v4, "sku"

    invoke-virtual {v2}, Lcom/amazon/device/iap/model/Receipt;->getSku()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v1, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 200
    const-string v4, "receipt"

    invoke-direct {p0, v2}, Lcom/outlinegames/unibillAmazon/Unibill;->encodeReceipt(Lcom/amazon/device/iap/model/Receipt;)Lorg/json/JSONObject;

    move-result-object v5

    invoke-virtual {v5}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v1, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 201
    invoke-virtual {v2}, Lcom/amazon/device/iap/model/Receipt;->getReceiptId()Ljava/lang/String;

    move-result-object v4

    sget-object v5, Lcom/amazon/device/iap/model/FulfillmentResult;->FULFILLED:Lcom/amazon/device/iap/model/FulfillmentResult;

    invoke-static {v4, v5}, Lcom/amazon/device/iap/PurchasingService;->notifyFulfillment(Ljava/lang/String;Lcom/amazon/device/iap/model/FulfillmentResult;)V
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_0

    goto :goto_1

    .line 204
    .end local v1    # "purchase":Lorg/json/JSONObject;
    .end local v2    # "receipt":Lcom/amazon/device/iap/model/Receipt;
    :catch_0
    move-exception v0

    .line 205
    .local v0, "j":Lorg/json/JSONException;
    const-string v3, "UnibillAmazonPlugin"

    invoke-virtual {v0}, Lorg/json/JSONException;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-static {v3, v4}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_2

    .line 211
    .end local v0    # "j":Lorg/json/JSONException;
    :cond_4
    const-string v3, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v4, "onPurchaseUpdateSuccess"

    iget-object v5, p0, Lcom/outlinegames/unibillAmazon/Unibill;->jsonResponse:Lorg/json/JSONObject;

    invoke-virtual {v5}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-static {v3, v4, v5}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 212
    iget-boolean v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restorePending:Z

    if-eqz v3, :cond_0

    .line 213
    const-string v3, "AmazonAppStoreCallbackMonoBehaviour"

    const-string v4, "onTransactionsRestored"

    const-string v5, "true"

    invoke-static {v3, v4, v5}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 214
    iput-boolean v6, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restorePending:Z

    goto :goto_0
.end method

.method public onUserDataResponse(Lcom/amazon/device/iap/model/UserDataResponse;)V
    .locals 9
    .param p1, "response"    # Lcom/amazon/device/iap/model/UserDataResponse;

    .prologue
    const/4 v8, 0x1

    const/4 v7, 0x0

    .line 221
    const-string v3, "UnibillAmazonPlugin"

    const-string v4, "onGetUserIdResponse:%s"

    new-array v5, v8, [Ljava/lang/Object;

    invoke-virtual {p1}, Lcom/amazon/device/iap/model/UserDataResponse;->getUserData()Lcom/amazon/device/iap/model/UserData;

    move-result-object v6

    invoke-virtual {v6}, Lcom/amazon/device/iap/model/UserData;->getUserId()Ljava/lang/String;

    move-result-object v6

    aput-object v6, v5, v7

    invoke-static {v4, v5}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v4

    invoke-static {v3, v4}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 222
    sget-object v3, Lcom/amazon/device/iap/model/UserDataResponse$RequestStatus;->SUCCESSFUL:Lcom/amazon/device/iap/model/UserDataResponse$RequestStatus;

    invoke-virtual {p1}, Lcom/amazon/device/iap/model/UserDataResponse;->getRequestStatus()Lcom/amazon/device/iap/model/UserDataResponse$RequestStatus;

    move-result-object v4

    if-ne v3, v4, :cond_0

    .line 223
    invoke-virtual {p1}, Lcom/amazon/device/iap/model/UserDataResponse;->getUserData()Lcom/amazon/device/iap/model/UserData;

    move-result-object v3

    invoke-virtual {v3}, Lcom/amazon/device/iap/model/UserData;->getUserId()Ljava/lang/String;

    move-result-object v3

    sput-object v3, Lcom/outlinegames/unibillAmazon/Unibill;->userId:Ljava/lang/String;

    .line 227
    :cond_0
    new-instance v3, Ljava/util/LinkedList;

    invoke-direct {v3}, Ljava/util/LinkedList;-><init>()V

    iput-object v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    .line 228
    iget-object v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->items:Ljava/util/ArrayList;

    invoke-virtual {v3}, Ljava/util/ArrayList;->size()I

    move-result v2

    .line 229
    .local v2, "sizeOfList":I
    const/16 v0, 0x64

    .line 231
    .local v0, "breakApart":I
    const/4 v1, 0x0

    .local v1, "i":I
    :goto_0
    if-lt v1, v2, :cond_1

    .line 236
    const-string v3, "UnibillAmazonPlugin"

    const-string v4, "Requesting %d groups of skus"

    new-array v5, v8, [Ljava/lang/Object;

    iget-object v6, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    invoke-interface {v6}, Ljava/util/Queue;->size()I

    move-result v6

    invoke-static {v6}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v6

    aput-object v6, v5, v7

    invoke-static {v4, v5}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v4

    invoke-static {v3, v4}, Landroid/util/Log;->d(Ljava/lang/String;Ljava/lang/String;)I

    .line 237
    iget-object v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    invoke-interface {v3}, Ljava/util/Queue;->remove()Ljava/lang/Object;

    move-result-object v3

    check-cast v3, Ljava/util/Set;

    invoke-static {v3}, Lcom/amazon/device/iap/PurchasingService;->getProductData(Ljava/util/Set;)Lcom/amazon/device/iap/model/RequestId;

    .line 238
    return-void

    .line 232
    :cond_1
    iget-object v3, p0, Lcom/outlinegames/unibillAmazon/Unibill;->itemGroups:Ljava/util/Queue;

    new-instance v4, Ljava/util/HashSet;

    .line 233
    iget-object v5, p0, Lcom/outlinegames/unibillAmazon/Unibill;->items:Ljava/util/ArrayList;

    add-int/lit8 v6, v1, 0x64

    invoke-static {v2, v6}, Ljava/lang/Math;->min(II)I

    move-result v6

    invoke-virtual {v5, v1, v6}, Ljava/util/ArrayList;->subList(II)Ljava/util/List;

    move-result-object v5

    invoke-direct {v4, v5}, Ljava/util/HashSet;-><init>(Ljava/util/Collection;)V

    .line 232
    invoke-interface {v3, v4}, Ljava/util/Queue;->add(Ljava/lang/Object;)Z

    .line 231
    add-int/lit8 v1, v1, 0x64

    goto :goto_0
.end method

.method public restoreTransactions()V
    .locals 1
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    const/4 v0, 0x1

    .line 61
    iput-boolean v0, p0, Lcom/outlinegames/unibillAmazon/Unibill;->restorePending:Z

    .line 62
    invoke-direct {p0, v0}, Lcom/outlinegames/unibillAmazon/Unibill;->initiatePurchaseUpdate(Z)V

    .line 63
    return-void
.end method
