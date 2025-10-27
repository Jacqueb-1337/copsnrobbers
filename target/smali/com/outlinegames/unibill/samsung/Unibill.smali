.class public Lcom/outlinegames/unibill/samsung/Unibill;
.super Ljava/lang/Object;
.source "Unibill.java"

# interfaces
.implements Lcom/sec/android/iap/lib/listener/OnGetItemListener;
.implements Lcom/sec/android/iap/lib/listener/OnPaymentListener;
.implements Lcom/sec/android/iap/lib/listener/OnGetInboxListener;
.implements Lcom/sec/android/iap/lib/listener/OnIapBindListener;


# static fields
.field private static final UNIBILL_GAMEOBJECT_NAME:Ljava/lang/String; = "SamsungAppsCallbackMonoBehaviour"

.field public static final UNIBILL_LOG_PREFIX:Ljava/lang/String; = "UnibillSamsung"

.field private static final UNITY_METHOD_NAME_INIT_FAIL:Ljava/lang/String; = "onInitFail"

.field private static final UNITY_METHOD_NAME_PRODUCT_LIST_RECEIVED:Ljava/lang/String; = "onProductListReceived"

.field private static final UNITY_METHOD_NAME_PURCHASE_CANCELLED:Ljava/lang/String; = "onPurchaseCancelled"

.field private static final UNITY_METHOD_NAME_PURCHASE_FAILED:Ljava/lang/String; = "onPurchaseFailed"

.field private static final UNITY_METHOD_NAME_PURCHASE_SUCCESS:Ljava/lang/String; = "onPurchaseSucceeded"

.field private static final UNITY_METHOD_NAME_TRANSACTIONS_RESTORED:Ljava/lang/String; = "onTransactionsRestored"

.field private static volatile instance:Lcom/outlinegames/unibill/samsung/Unibill;


# instance fields
.field private volatile executor:Ljava/util/concurrent/ExecutorService;

.field private volatile helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

.field private volatile itemGroupId:Ljava/lang/String;

.field private mode:I

.field private productIdUnderPurchase:Ljava/lang/String;


# direct methods
.method public constructor <init>()V
    .locals 1

    .prologue
    .line 28
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 43
    const/4 v0, 0x0

    iput v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    .line 45
    invoke-static {}, Ljava/util/concurrent/Executors;->newSingleThreadExecutor()Ljava/util/concurrent/ExecutorService;

    move-result-object v0

    iput-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->executor:Ljava/util/concurrent/ExecutorService;

    .line 28
    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/samsung/Unibill;Ljava/lang/String;)V
    .locals 0

    .prologue
    .line 100
    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    return-void
.end method

.method static synthetic access$1(Lcom/outlinegames/unibill/samsung/Unibill;)Lcom/sec/android/iap/lib/helper/SamsungIapHelper;
    .locals 1

    .prologue
    .line 41
    iget-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    return-object v0
.end method

.method static synthetic access$2(Lcom/outlinegames/unibill/samsung/Unibill;)Ljava/lang/String;
    .locals 1

    .prologue
    .line 42
    iget-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->itemGroupId:Ljava/lang/String;

    return-object v0
.end method

.method static synthetic access$3(Lcom/outlinegames/unibill/samsung/Unibill;)I
    .locals 1

    .prologue
    .line 43
    iget v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    return v0
.end method

.method private getItemList(J)V
    .locals 3
    .param p1, "delayInMilliseconds"    # J

    .prologue
    .line 74
    iget-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->executor:Ljava/util/concurrent/ExecutorService;

    new-instance v1, Lcom/outlinegames/unibill/samsung/Unibill$1;

    invoke-direct {v1, p0, p1, p2}, Lcom/outlinegames/unibill/samsung/Unibill$1;-><init>(Lcom/outlinegames/unibill/samsung/Unibill;J)V

    invoke-interface {v0, v1}, Ljava/util/concurrent/ExecutorService;->execute(Ljava/lang/Runnable;)V

    .line 88
    return-void
.end method

.method public static instance()Lcom/outlinegames/unibill/samsung/Unibill;
    .locals 1

    .prologue
    .line 48
    sget-object v0, Lcom/outlinegames/unibill/samsung/Unibill;->instance:Lcom/outlinegames/unibill/samsung/Unibill;

    if-nez v0, :cond_0

    .line 49
    new-instance v0, Lcom/outlinegames/unibill/samsung/Unibill;

    invoke-direct {v0}, Lcom/outlinegames/unibill/samsung/Unibill;-><init>()V

    sput-object v0, Lcom/outlinegames/unibill/samsung/Unibill;->instance:Lcom/outlinegames/unibill/samsung/Unibill;

    .line 51
    :cond_0
    sget-object v0, Lcom/outlinegames/unibill/samsung/Unibill;->instance:Lcom/outlinegames/unibill/samsung/Unibill;

    return-object v0
.end method

.method private log(Ljava/lang/String;)V
    .locals 1
    .param p1, "message"    # Ljava/lang/String;

    .prologue
    .line 101
    const-string v0, "UnibillSamsung"

    invoke-static {v0, p1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 102
    return-void
.end method

.method private log(Ljava/lang/String;Ljava/lang/String;)V
    .locals 2
    .param p1, "message"    # Ljava/lang/String;
    .param p2, "arg"    # Ljava/lang/String;

    .prologue
    .line 105
    const/4 v0, 0x1

    new-array v0, v0, [Ljava/lang/Object;

    const/4 v1, 0x0

    aput-object p2, v0, v1

    invoke-static {p1, v0}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 106
    return-void
.end method

.method private notifyPurchase(Ljava/lang/String;Ljava/lang/String;)V
    .locals 3
    .param p1, "productId"    # Ljava/lang/String;
    .param p2, "purchaseId"    # Ljava/lang/String;

    .prologue
    .line 188
    new-instance v0, Lcom/outlinegames/unibill/samsung/SaneJSONObject;

    invoke-direct {v0}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;-><init>()V

    .line 189
    .local v0, "json":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    const-string v1, "productId"

    invoke-virtual {v0, v1, p1}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 190
    const-string v1, "signature"

    invoke-virtual {v0, v1, p2}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 191
    const-string v1, "onPurchaseSucceeded"

    .line 192
    invoke-virtual {v0}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->toString()Ljava/lang/String;

    move-result-object v2

    .line 191
    invoke-virtual {p0, v1, v2}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    .line 193
    return-void
.end method


# virtual methods
.method public initialise(Ljava/lang/String;)V
    .locals 4
    .param p1, "unibillJSON"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 55
    const-string v2, "initialise: %s"

    invoke-direct {p0, v2, p1}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 56
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0, p1}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    .line 57
    .local v0, "json":Lorg/json/JSONObject;
    const-string v2, "itemGroupId"

    invoke-virtual {v0, v2}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    iput-object v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->itemGroupId:Ljava/lang/String;

    .line 59
    const-string v2, "mode"

    invoke-virtual {v0, v2}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    .line 60
    .local v1, "modeString":Ljava/lang/String;
    const-string v2, "PRODUCTION"

    invoke-virtual {v1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_1

    .line 61
    const/4 v2, 0x0

    iput v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    .line 69
    :cond_0
    :goto_0
    sget-object v2, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    iget v3, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    invoke-static {v2, v3}, Lcom/sec/android/iap/lib/helper/SamsungIapHelper;->getInstance(Landroid/content/Context;I)Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    move-result-object v2

    .line 68
    iput-object v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    .line 70
    iget-object v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    invoke-virtual {v2, p0}, Lcom/sec/android/iap/lib/helper/SamsungIapHelper;->bindIapService(Lcom/sec/android/iap/lib/listener/OnIapBindListener;)V

    .line 71
    return-void

    .line 62
    :cond_1
    const-string v2, "ALWAYS_SUCCEED"

    invoke-virtual {v1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_2

    .line 63
    const/4 v2, 0x1

    iput v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    goto :goto_0

    .line 64
    :cond_2
    const-string v2, "ALWAYS_FAIL"

    invoke-virtual {v1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_0

    .line 65
    const/4 v2, -0x1

    iput v2, p0, Lcom/outlinegames/unibill/samsung/Unibill;->mode:I

    goto :goto_0
.end method

.method public initiatePurchaseRequest(Ljava/lang/String;)V
    .locals 3
    .param p1, "productId"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 116
    iput-object p1, p0, Lcom/outlinegames/unibill/samsung/Unibill;->productIdUnderPurchase:Ljava/lang/String;

    .line 117
    iget-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    iget-object v1, p0, Lcom/outlinegames/unibill/samsung/Unibill;->itemGroupId:Ljava/lang/String;

    const/4 v2, 0x0

    invoke-virtual {v0, v1, p1, v2, p0}, Lcom/sec/android/iap/lib/helper/SamsungIapHelper;->startPayment(Ljava/lang/String;Ljava/lang/String;ZLcom/sec/android/iap/lib/listener/OnPaymentListener;)V

    .line 118
    return-void
.end method

.method public onBindIapFinished(I)V
    .locals 2
    .param p1, "result"    # I

    .prologue
    .line 197
    const-string v0, "onBindIapFinished:%s"

    invoke-static {p1}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v1

    invoke-direct {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 198
    if-nez p1, :cond_0

    .line 199
    const-string v0, "Initialised successfully!"

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 200
    const-wide/16 v0, 0x0

    invoke-direct {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->getItemList(J)V

    .line 204
    :goto_0
    return-void

    .line 202
    :cond_0
    const-string v0, "onInitFail"

    const-string v1, ""

    invoke-virtual {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0
.end method

.method public onGetItem(Lcom/sec/android/iap/lib/vo/ErrorVo;Ljava/util/ArrayList;)V
    .locals 7
    .param p1, "error"    # Lcom/sec/android/iap/lib/vo/ErrorVo;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/sec/android/iap/lib/vo/ErrorVo;",
            "Ljava/util/ArrayList",
            "<",
            "Lcom/sec/android/iap/lib/vo/ItemVo;",
            ">;)V"
        }
    .end annotation

    .prologue
    .line 122
    .local p2, "itemList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Lcom/sec/android/iap/lib/vo/ItemVo;>;"
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "onGetItem response:"

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v5

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-direct {p0, v4}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 123
    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v4

    if-nez v4, :cond_1

    .line 124
    new-instance v2, Lcom/outlinegames/unibill/samsung/SaneJSONObject;

    invoke-direct {v2}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;-><init>()V

    .line 125
    .local v2, "response":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    invoke-virtual {p2}, Ljava/util/ArrayList;->iterator()Ljava/util/Iterator;

    move-result-object v4

    :goto_0
    invoke-interface {v4}, Ljava/util/Iterator;->hasNext()Z

    move-result v5

    if-nez v5, :cond_0

    .line 137
    invoke-virtual {v2}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->toString()Ljava/lang/String;

    move-result-object v3

    .line 139
    .local v3, "str":Ljava/lang/String;
    const-string v4, "onProductListReceived"

    .line 138
    invoke-virtual {p0, v4, v3}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    .line 146
    .end local v2    # "response":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    .end local v3    # "str":Ljava/lang/String;
    :goto_1
    return-void

    .line 125
    .restart local v2    # "response":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    :cond_0
    invoke-interface {v4}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/sec/android/iap/lib/vo/ItemVo;

    .line 126
    .local v0, "item":Lcom/sec/android/iap/lib/vo/ItemVo;
    new-instance v1, Lcom/outlinegames/unibill/samsung/SaneJSONObject;

    invoke-direct {v1}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;-><init>()V

    .line 127
    .local v1, "product":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    const-string v5, "price"

    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/ItemVo;->getItemPriceString()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v1, v5, v6}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 128
    const-string v5, "localizedTitle"

    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/ItemVo;->getItemName()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v1, v5, v6}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 129
    const-string v5, "localizedDescription"

    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/ItemVo;->getItemDesc()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v1, v5, v6}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 131
    const-string v5, "isoCurrencyCode"

    invoke-static {}, Ljava/util/Locale;->getDefault()Ljava/util/Locale;

    move-result-object v6

    invoke-static {v6}, Ljava/util/Currency;->getInstance(Ljava/util/Locale;)Ljava/util/Currency;

    move-result-object v6

    invoke-virtual {v6}, Ljava/util/Currency;->getCurrencyCode()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v1, v5, v6}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 132
    const-string v5, "priceDecimal"

    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/ItemVo;->getItemPrice()Ljava/lang/Double;

    move-result-object v6

    invoke-virtual {v1, v5, v6}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 134
    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/ItemVo;->getItemId()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v5, v1}, Lcom/outlinegames/unibill/samsung/SaneJSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    goto :goto_0

    .line 142
    .end local v0    # "item":Lcom/sec/android/iap/lib/vo/ItemVo;
    .end local v1    # "product":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    .end local v2    # "response":Lcom/outlinegames/unibill/samsung/SaneJSONObject;
    :cond_1
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Error message: "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorString()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-direct {p0, v4}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 143
    const-string v4, "Failure retrieving item list. Unibill will retry in 10 seconds..."

    invoke-direct {p0, v4}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 144
    const-wide/16 v4, 0x2710

    invoke-direct {p0, v4, v5}, Lcom/outlinegames/unibill/samsung/Unibill;->getItemList(J)V

    goto :goto_1
.end method

.method public onGetItemInbox(Lcom/sec/android/iap/lib/vo/ErrorVo;Ljava/util/ArrayList;)V
    .locals 4
    .param p1, "error"    # Lcom/sec/android/iap/lib/vo/ErrorVo;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/sec/android/iap/lib/vo/ErrorVo;",
            "Ljava/util/ArrayList",
            "<",
            "Lcom/sec/android/iap/lib/vo/InboxVo;",
            ">;)V"
        }
    .end annotation

    .prologue
    .line 170
    .local p2, "inbox":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Lcom/sec/android/iap/lib/vo/InboxVo;>;"
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "onGetItem response:"

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {p0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 171
    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v1

    if-nez v1, :cond_3

    .line 172
    invoke-virtual {p2}, Ljava/util/ArrayList;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :cond_0
    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v2

    if-nez v2, :cond_1

    .line 179
    const-string v1, "onTransactionsRestored"

    const-string v2, "true"

    .line 178
    invoke-virtual {p0, v1, v2}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    .line 185
    :goto_1
    return-void

    .line 172
    :cond_1
    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/sec/android/iap/lib/vo/InboxVo;

    .line 174
    .local v0, "item":Lcom/sec/android/iap/lib/vo/InboxVo;
    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/InboxVo;->getType()Ljava/lang/String;

    move-result-object v2

    const-string v3, "01"

    invoke-virtual {v2, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_2

    const-string v2, "02"

    invoke-virtual {v0, v2}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_0

    .line 175
    :cond_2
    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/InboxVo;->getItemId()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0}, Lcom/sec/android/iap/lib/vo/InboxVo;->getJsonString()Ljava/lang/String;

    move-result-object v3

    invoke-direct {p0, v2, v3}, Lcom/outlinegames/unibill/samsung/Unibill;->notifyPurchase(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 181
    .end local v0    # "item":Lcom/sec/android/iap/lib/vo/InboxVo;
    :cond_3
    const-string v1, "Failure retrieving item list"

    invoke-direct {p0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 183
    const-string v1, "onTransactionsRestored"

    const-string v2, "false"

    .line 182
    invoke-virtual {p0, v1, v2}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_1
.end method

.method public onPayment(Lcom/sec/android/iap/lib/vo/ErrorVo;Lcom/sec/android/iap/lib/vo/PurchaseVo;)V
    .locals 2
    .param p1, "error"    # Lcom/sec/android/iap/lib/vo/ErrorVo;
    .param p2, "purchase"    # Lcom/sec/android/iap/lib/vo/PurchaseVo;

    .prologue
    .line 150
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Purchase result:"

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 151
    invoke-virtual {p1}, Lcom/sec/android/iap/lib/vo/ErrorVo;->getErrorCode()I

    move-result v0

    sparse-switch v0, :sswitch_data_0

    .line 162
    const-string v0, "onPurchaseFailed"

    .line 163
    iget-object v1, p0, Lcom/outlinegames/unibill/samsung/Unibill;->productIdUnderPurchase:Ljava/lang/String;

    .line 162
    invoke-virtual {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    .line 166
    :goto_0
    return-void

    .line 154
    :sswitch_0
    invoke-virtual {p2}, Lcom/sec/android/iap/lib/vo/PurchaseVo;->getItemId()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p2}, Lcom/sec/android/iap/lib/vo/PurchaseVo;->getPurchaseId()Ljava/lang/String;

    move-result-object v1

    invoke-direct {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->notifyPurchase(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 158
    :sswitch_1
    const-string v0, "onPurchaseCancelled"

    .line 159
    iget-object v1, p0, Lcom/outlinegames/unibill/samsung/Unibill;->productIdUnderPurchase:Ljava/lang/String;

    .line 157
    invoke-virtual {p0, v0, v1}, Lcom/outlinegames/unibill/samsung/Unibill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 151
    nop

    :sswitch_data_0
    .sparse-switch
        -0x3eb -> :sswitch_0
        0x0 -> :sswitch_0
        0x1 -> :sswitch_1
    .end sparse-switch
.end method

.method public restoreTransactions()V
    .locals 9

    .prologue
    .line 91
    const-string v0, "Restoring transactions..."

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/samsung/Unibill;->log(Ljava/lang/String;)V

    .line 92
    new-instance v7, Ljava/util/Date;

    invoke-direct {v7}, Ljava/util/Date;-><init>()V

    .line 93
    .local v7, "d":Ljava/util/Date;
    new-instance v8, Ljava/text/SimpleDateFormat;

    const-string v0, "yyyyMMdd"

    .line 94
    invoke-static {}, Ljava/util/Locale;->getDefault()Ljava/util/Locale;

    move-result-object v1

    .line 93
    invoke-direct {v8, v0, v1}, Ljava/text/SimpleDateFormat;-><init>(Ljava/lang/String;Ljava/util/Locale;)V

    .line 95
    .local v8, "sdf":Ljava/text/SimpleDateFormat;
    invoke-virtual {v8, v7}, Ljava/text/SimpleDateFormat;->format(Ljava/util/Date;)Ljava/lang/String;

    move-result-object v5

    .line 96
    .local v5, "today":Ljava/lang/String;
    iget-object v0, p0, Lcom/outlinegames/unibill/samsung/Unibill;->helper:Lcom/sec/android/iap/lib/helper/SamsungIapHelper;

    iget-object v1, p0, Lcom/outlinegames/unibill/samsung/Unibill;->itemGroupId:Ljava/lang/String;

    const/4 v2, 0x1

    const v3, 0x7fffffff

    const-string v4, "20130101"

    move-object v6, p0

    invoke-virtual/range {v0 .. v6}, Lcom/sec/android/iap/lib/helper/SamsungIapHelper;->getItemInboxList(Ljava/lang/String;IILjava/lang/String;Ljava/lang/String;Lcom/sec/android/iap/lib/listener/OnGetInboxListener;)V

    .line 98
    return-void
.end method

.method public sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1
    .param p1, "methodName"    # Ljava/lang/String;
    .param p2, "body"    # Ljava/lang/String;

    .prologue
    .line 109
    const-string v0, "SamsungAppsCallbackMonoBehaviour"

    invoke-static {v0, p1, p2}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 110
    return-void
.end method
