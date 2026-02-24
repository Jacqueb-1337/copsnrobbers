.class public Lcom/outlinegames/unibill/UniBill;
.super Ljava/lang/Object;
.source "UniBill.java"


# static fields
.field public static final UNIBILL_ANDROID_PK_KEY:Ljava/lang/String; = "UnibillAPKK"

.field private static final UNIBILL_GAMEOBJECT_NAME:Ljava/lang/String; = "GooglePlayCallbackMonoBehaviour"

.field public static final UNIBILL_LOG_PREFIX:Ljava/lang/String; = "Unibill"

.field private static final UNIBILL_SHARED_PREFS_NAME:Ljava/lang/String; = "UnibillSharedPrefs"

.field private static final UNITY_METHOD_NAME_BILLING_NOT_SUPPORTED:Ljava/lang/String; = "onBillingNotSupported"

.field private static final UNITY_METHOD_NAME_POLL_FOR_CONSUMABLES_FINISHED:Ljava/lang/String; = "onPollForConsumablesFinished"

.field private static final UNITY_METHOD_NAME_PRODUCT_LIST_RECEIVED:Ljava/lang/String; = "onProductListReceived"

.field private static final UNITY_METHOD_NAME_PURCHASE_CANCELLED:Ljava/lang/String; = "onPurchaseCancelled"

.field private static final UNITY_METHOD_NAME_PURCHASE_FAILED:Ljava/lang/String; = "onPurchaseFailed"

.field private static final UNITY_METHOD_NAME_PURCHASE_SUCCESS:Ljava/lang/String; = "onPurchaseSucceeded"

.field private static final UNITY_METHOD_NAME_TRANSACTIONS_RESTORED:Ljava/lang/String; = "onTransactionsRestored"

.field private static instance:Lcom/outlinegames/unibill/UniBill;


# instance fields
.field public PurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

.field public activityPending:Z

.field private consumables:Ljava/util/Set;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Set",
            "<",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation
.end field

.field public helper:Lcom/outlinegames/unibill/IabHelper;

.field private inventory:Lcom/outlinegames/unibill/Inventory;

.field private json:Lorg/json/JSONObject;

.field private offlineBackOffTime:I

.field private volatile purchaseInProgress:Z

.field private skuUnderPurchase:Ljava/lang/String;


# direct methods
.method public constructor <init>()V
    .locals 1

    .prologue
    .line 25
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 60
    new-instance v0, Lcom/outlinegames/unibill/UniBill$1;

    invoke-direct {v0, p0}, Lcom/outlinegames/unibill/UniBill$1;-><init>(Lcom/outlinegames/unibill/UniBill;)V

    iput-object v0, p0, Lcom/outlinegames/unibill/UniBill;->PurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    .line 138
    const/16 v0, 0x1388

    iput v0, p0, Lcom/outlinegames/unibill/UniBill;->offlineBackOffTime:I

    .line 221
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/outlinegames/unibill/UniBill;->purchaseInProgress:Z

    .line 25
    return-void
.end method

.method private ConsumeProductAndTellUnity(Lcom/outlinegames/unibill/Purchase;J)V
    .locals 2
    .param p1, "purchase"    # Lcom/outlinegames/unibill/Purchase;
    .param p2, "delay"    # J

    .prologue
    .line 298
    const-string v0, "ConsumeProductAndTellUnity:%s"

    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getSku()Ljava/lang/String;

    move-result-object v1

    invoke-direct {p0, v0, v1}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 299
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    new-instance v1, Lcom/outlinegames/unibill/UniBill$6;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/UniBill$6;-><init>(Lcom/outlinegames/unibill/UniBill;)V

    invoke-virtual {v0, p1, p2, p3, v1}, Lcom/outlinegames/unibill/IabHelper;->consumeAsync(Lcom/outlinegames/unibill/Purchase;JLcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;)V

    .line 316
    return-void
.end method

.method private NotifyUnityOfPurchase(Lcom/outlinegames/unibill/Purchase;)V
    .locals 4
    .param p1, "purchase"    # Lcom/outlinegames/unibill/Purchase;

    .prologue
    .line 319
    const-string v2, "NotifyUnityOfPurchase"

    invoke-direct {p0, v2}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    .line 321
    :try_start_0
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 322
    .local v1, "o":Lorg/json/JSONObject;
    const-string v2, "productId"

    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getSku()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 323
    const-string v2, "signature"

    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/UniBill;->encodeReceipt(Lcom/outlinegames/unibill/Purchase;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 324
    const-string v2, "onPurchaseSucceeded"

    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {p0, v2, v3}, Lcom/outlinegames/unibill/UniBill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 328
    .end local v1    # "o":Lorg/json/JSONObject;
    :goto_0
    return-void

    .line 325
    :catch_0
    move-exception v0

    .line 326
    .local v0, "j":Lorg/json/JSONException;
    invoke-virtual {v0}, Lorg/json/JSONException;->getMessage()Ljava/lang/String;

    move-result-object v2

    invoke-direct {p0, v2}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    goto :goto_0
.end method

.method private QueryInventory(Ljava/util/List;J)V
    .locals 6
    .param p2, "delay"    # J
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;J)V"
        }
    .end annotation

    .prologue
    .line 141
    .local p1, "skus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    const-string v0, "QueryInventory: %s"

    invoke-interface {p1}, Ljava/util/List;->size()I

    move-result v1

    invoke-static {v1}, Ljava/lang/Integer;->toString(I)Ljava/lang/String;

    move-result-object v1

    invoke-direct {p0, v0, v1}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 142
    new-instance v3, Lcom/outlinegames/unibill/UniBill$3;

    invoke-direct {v3, p0, p1}, Lcom/outlinegames/unibill/UniBill$3;-><init>(Lcom/outlinegames/unibill/UniBill;Ljava/util/List;)V

    .line 175
    .local v3, "listener":Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    const/4 v1, 0x1

    move-object v2, p1

    move-wide v4, p2

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->queryInventoryAsync(ZLjava/util/List;Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;J)V

    .line 176
    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;Ljava/lang/String;)V
    .locals 0

    .prologue
    .line 362
    invoke-direct {p0, p1, p2}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method static synthetic access$1(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;)V
    .locals 0

    .prologue
    .line 358
    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    return-void
.end method

.method static synthetic access$10(Lcom/outlinegames/unibill/UniBill;)I
    .locals 1

    .prologue
    .line 138
    iget v0, p0, Lcom/outlinegames/unibill/UniBill;->offlineBackOffTime:I

    return v0
.end method

.method static synthetic access$11(Lcom/outlinegames/unibill/UniBill;I)V
    .locals 0

    .prologue
    .line 138
    iput p1, p0, Lcom/outlinegames/unibill/UniBill;->offlineBackOffTime:I

    return-void
.end method

.method static synthetic access$12(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Inventory;)V
    .locals 0

    .prologue
    .line 47
    iput-object p1, p0, Lcom/outlinegames/unibill/UniBill;->inventory:Lcom/outlinegames/unibill/Inventory;

    return-void
.end method

.method static synthetic access$13(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Purchase;)Ljava/lang/String;
    .locals 1

    .prologue
    .line 330
    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/UniBill;->encodeReceipt(Lcom/outlinegames/unibill/Purchase;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method static synthetic access$2(Lcom/outlinegames/unibill/UniBill;Z)V
    .locals 0

    .prologue
    .line 221
    iput-boolean p1, p0, Lcom/outlinegames/unibill/UniBill;->purchaseInProgress:Z

    return-void
.end method

.method static synthetic access$3(Lcom/outlinegames/unibill/UniBill;)Ljava/util/Set;
    .locals 1

    .prologue
    .line 48
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->consumables:Ljava/util/Set;

    return-object v0
.end method

.method static synthetic access$4(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Purchase;J)V
    .locals 0

    .prologue
    .line 297
    invoke-direct {p0, p1, p2, p3}, Lcom/outlinegames/unibill/UniBill;->ConsumeProductAndTellUnity(Lcom/outlinegames/unibill/Purchase;J)V

    return-void
.end method

.method static synthetic access$5(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Purchase;)V
    .locals 0

    .prologue
    .line 318
    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/UniBill;->NotifyUnityOfPurchase(Lcom/outlinegames/unibill/Purchase;)V

    return-void
.end method

.method static synthetic access$6(Lcom/outlinegames/unibill/UniBill;)Ljava/lang/String;
    .locals 1

    .prologue
    .line 49
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->skuUnderPurchase:Ljava/lang/String;

    return-object v0
.end method

.method static synthetic access$7(Lcom/outlinegames/unibill/UniBill;Ljava/util/Set;)V
    .locals 0

    .prologue
    .line 48
    iput-object p1, p0, Lcom/outlinegames/unibill/UniBill;->consumables:Ljava/util/Set;

    return-void
.end method

.method static synthetic access$8(Lcom/outlinegames/unibill/UniBill;)Lorg/json/JSONObject;
    .locals 1

    .prologue
    .line 94
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->json:Lorg/json/JSONObject;

    return-object v0
.end method

.method static synthetic access$9(Lcom/outlinegames/unibill/UniBill;Ljava/util/List;J)V
    .locals 0

    .prologue
    .line 140
    invoke-direct {p0, p1, p2, p3}, Lcom/outlinegames/unibill/UniBill;->QueryInventory(Ljava/util/List;J)V

    return-void
.end method

.method private encodeReceipt(Lcom/outlinegames/unibill/Purchase;)Ljava/lang/String;
    .locals 4
    .param p1, "purchase"    # Lcom/outlinegames/unibill/Purchase;

    .prologue
    .line 331
    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    .line 333
    .local v1, "signature":Lorg/json/JSONObject;
    :try_start_0
    const-string v2, "json"

    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getOriginalJson()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 334
    const-string v2, "signature"

    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getSignature()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    .line 335
    const-string v2, "developerPayload"

    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getDeveloperPayload()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 341
    :goto_0
    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v2

    return-object v2

    .line 336
    :catch_0
    move-exception v0

    .line 338
    .local v0, "e":Lorg/json/JSONException;
    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method private getActivity()Landroid/app/Activity;
    .locals 1

    .prologue
    .line 368
    sget-object v0, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    return-object v0
.end method

.method private getSharedPrefs()Landroid/content/SharedPreferences;
    .locals 3

    .prologue
    .line 373
    sget-object v0, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    const-string v1, "UnibillSharedPrefs"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Landroid/app/Activity;->getSharedPreferences(Ljava/lang/String;I)Landroid/content/SharedPreferences;

    move-result-object v0

    return-object v0
.end method

.method public static instance()Lcom/outlinegames/unibill/UniBill;
    .locals 1

    .prologue
    .line 54
    sget-object v0, Lcom/outlinegames/unibill/UniBill;->instance:Lcom/outlinegames/unibill/UniBill;

    if-nez v0, :cond_0

    .line 55
    new-instance v0, Lcom/outlinegames/unibill/UniBill;

    invoke-direct {v0}, Lcom/outlinegames/unibill/UniBill;-><init>()V

    sput-object v0, Lcom/outlinegames/unibill/UniBill;->instance:Lcom/outlinegames/unibill/UniBill;

    .line 57
    :cond_0
    sget-object v0, Lcom/outlinegames/unibill/UniBill;->instance:Lcom/outlinegames/unibill/UniBill;

    return-object v0
.end method

.method private log(Ljava/lang/String;)V
    .locals 1
    .param p1, "message"    # Ljava/lang/String;

    .prologue
    .line 359
    const-string v0, "Unibill"

    invoke-static {v0, p1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 360
    return-void
.end method

.method private log(Ljava/lang/String;Ljava/lang/String;)V
    .locals 2
    .param p1, "message"    # Ljava/lang/String;
    .param p2, "arg"    # Ljava/lang/String;

    .prologue
    .line 363
    const/4 v0, 0x1

    new-array v0, v0, [Ljava/lang/Object;

    const/4 v1, 0x0

    aput-object p2, v0, v1

    invoke-static {p1, v0}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    .line 364
    return-void
.end method


# virtual methods
.method public Dispose()V
    .locals 1

    .prologue
    .line 132
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    if-eqz v0, :cond_0

    .line 133
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    invoke-virtual {v0}, Lcom/outlinegames/unibill/IabHelper;->dispose()V

    .line 134
    const/4 v0, 0x0

    iput-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    .line 136
    :cond_0
    return-void
.end method

.method public initialise(Ljava/lang/String;)V
    .locals 4
    .param p1, "unibillJSON"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 96
    const-string v0, "initialise: %s"

    invoke-direct {p0, v0, p1}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 97
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0, p1}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    iput-object v0, p0, Lcom/outlinegames/unibill/UniBill;->json:Lorg/json/JSONObject;

    .line 99
    new-instance v0, Lcom/outlinegames/unibill/IabHelper;

    sget-object v1, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    iget-object v2, p0, Lcom/outlinegames/unibill/UniBill;->json:Lorg/json/JSONObject;

    const-string v3, "publicKey"

    invoke-virtual {v2, v3}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Lcom/outlinegames/unibill/IabHelper;-><init>(Landroid/content/Context;Ljava/lang/String;)V

    iput-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    .line 101
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    new-instance v1, Lcom/outlinegames/unibill/UniBill$2;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/UniBill$2;-><init>(Lcom/outlinegames/unibill/UniBill;)V

    invoke-virtual {v0, v1}, Lcom/outlinegames/unibill/IabHelper;->startSetup(Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;)V

    .line 129
    return-void
.end method

.method protected onActivityResult(IILandroid/content/Intent;)V
    .locals 1
    .param p1, "requestCode"    # I
    .param p2, "resultCode"    # I
    .param p3, "data"    # Landroid/content/Intent;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 214
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    if-eqz v0, :cond_0

    .line 215
    const-string v0, "onActivityResult"

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    .line 216
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    invoke-virtual {v0, p1, p2, p3}, Lcom/outlinegames/unibill/IabHelper;->handleActivityResult(IILandroid/content/Intent;)Z

    .line 217
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/outlinegames/unibill/UniBill;->purchaseInProgress:Z

    .line 219
    :cond_0
    return-void
.end method

.method public persistValue(Ljava/lang/String;Ljava/lang/String;)V
    .locals 4
    .param p1, "key"    # Ljava/lang/String;
    .param p2, "value"    # Ljava/lang/String;

    .prologue
    .line 351
    :try_start_0
    invoke-direct {p0}, Lcom/outlinegames/unibill/UniBill;->getSharedPrefs()Landroid/content/SharedPreferences;

    move-result-object v1

    invoke-interface {v1}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v1

    invoke-interface {v1, p1, p2}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v1

    invoke-interface {v1}, Landroid/content/SharedPreferences$Editor;->commit()Z
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    .line 356
    :goto_0
    return-void

    .line 353
    :catch_0
    move-exception v0

    .line 354
    .local v0, "e":Ljava/lang/Exception;
    const-string v1, "Unibill"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "error persisting:"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Ljava/lang/Exception;->getMessage()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Landroid/util/Log;->e(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_0
.end method

.method public pollForConsumables()V
    .locals 2

    .prologue
    .line 249
    invoke-direct {p0}, Lcom/outlinegames/unibill/UniBill;->getActivity()Landroid/app/Activity;

    move-result-object v0

    new-instance v1, Lcom/outlinegames/unibill/UniBill$5;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/UniBill$5;-><init>(Lcom/outlinegames/unibill/UniBill;)V

    invoke-virtual {v0, v1}, Landroid/app/Activity;->runOnUiThread(Ljava/lang/Runnable;)V

    .line 295
    return-void
.end method

.method public purchaseProduct(Ljava/lang/String;)V
    .locals 8
    .param p1, "jsonRequest"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    const/4 v7, 0x1

    .line 223
    new-instance v4, Lorg/json/JSONObject;

    invoke-direct {v4, p1}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    .line 224
    .local v4, "req":Lorg/json/JSONObject;
    const-string v5, "productId"

    invoke-virtual {v4, v5}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    .line 225
    .local v2, "productId":Ljava/lang/String;
    const-string v5, "developerPayload"

    invoke-virtual {v4, v5}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    .line 227
    .local v1, "developerPayload":Ljava/lang/String;
    iget-object v5, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    iget-boolean v5, v5, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    if-nez v5, :cond_0

    iget-boolean v5, p0, Lcom/outlinegames/unibill/UniBill;->purchaseInProgress:Z

    if-eqz v5, :cond_1

    .line 228
    :cond_0
    const-string v5, "Purchase is already in progress! Failing purchase..."

    invoke-direct {p0, v5}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    .line 229
    const-string v5, "onPurchaseFailed"

    invoke-virtual {p0, v5, v2}, Lcom/outlinegames/unibill/UniBill;->sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V

    .line 246
    :goto_0
    return-void

    .line 233
    :cond_1
    iput-object v2, p0, Lcom/outlinegames/unibill/UniBill;->skuUnderPurchase:Ljava/lang/String;

    .line 234
    const-string v5, "onPurchaseProduct: %s"

    invoke-direct {p0, v5, v2}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 236
    iget-object v5, p0, Lcom/outlinegames/unibill/UniBill;->inventory:Lcom/outlinegames/unibill/Inventory;

    invoke-virtual {v5, v2}, Lcom/outlinegames/unibill/Inventory;->getSkuDetails(Ljava/lang/String;)Lcom/outlinegames/unibill/SkuDetails;

    move-result-object v0

    .line 237
    .local v0, "details":Lcom/outlinegames/unibill/SkuDetails;
    const-string v5, "ITEM TYPE:%s"

    invoke-virtual {v0}, Lcom/outlinegames/unibill/SkuDetails;->getType()Ljava/lang/String;

    move-result-object v6

    invoke-direct {p0, v5, v6}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;Ljava/lang/String;)V

    .line 239
    new-instance v3, Landroid/content/Intent;

    sget-object v5, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    const-class v6, Lcom/outlinegames/unibill/PurchaseActivity;

    invoke-direct {v3, v5, v6}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    .line 240
    .local v3, "purchaseIntent":Landroid/content/Intent;
    const-string v5, "productId"

    invoke-virtual {v3, v5, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    .line 241
    const-string v5, "itemType"

    invoke-virtual {v0}, Lcom/outlinegames/unibill/SkuDetails;->getType()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v3, v5, v6}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    .line 242
    const-string v5, "developerPayload"

    invoke-virtual {v3, v5, v1}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    .line 243
    iput-boolean v7, p0, Lcom/outlinegames/unibill/UniBill;->purchaseInProgress:Z

    .line 244
    iput-boolean v7, p0, Lcom/outlinegames/unibill/UniBill;->activityPending:Z

    .line 245
    sget-object v5, Lcom/unity3d/player/UnityPlayer;->currentActivity:Landroid/app/Activity;

    invoke-virtual {v5, v3}, Landroid/app/Activity;->startActivity(Landroid/content/Intent;)V

    goto :goto_0
.end method

.method public restoreTransactions()V
    .locals 2

    .prologue
    .line 179
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    iget-boolean v0, v0, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    if-eqz v0, :cond_0

    .line 180
    const-string v0, "Ignoring attempt to restore transactions whilst an operation is in progress..."

    invoke-direct {p0, v0}, Lcom/outlinegames/unibill/UniBill;->log(Ljava/lang/String;)V

    .line 211
    :goto_0
    return-void

    .line 183
    :cond_0
    invoke-direct {p0}, Lcom/outlinegames/unibill/UniBill;->getActivity()Landroid/app/Activity;

    move-result-object v0

    new-instance v1, Lcom/outlinegames/unibill/UniBill$4;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/UniBill$4;-><init>(Lcom/outlinegames/unibill/UniBill;)V

    invoke-virtual {v0, v1}, Landroid/app/Activity;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method public retrieveValue(Ljava/lang/String;)Ljava/lang/String;
    .locals 2
    .param p1, "key"    # Ljava/lang/String;

    .prologue
    .line 345
    invoke-direct {p0}, Lcom/outlinegames/unibill/UniBill;->getSharedPrefs()Landroid/content/SharedPreferences;

    move-result-object v0

    const/4 v1, 0x0

    invoke-interface {v0, p1, v1}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public sendMessageToUnityUnibillManager(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1
    .param p1, "methodName"    # Ljava/lang/String;
    .param p2, "body"    # Ljava/lang/String;

    .prologue
    .line 377
    const-string v0, "GooglePlayCallbackMonoBehaviour"

    invoke-static {v0, p1, p2}, Lcom/unity3d/player/UnityPlayer;->UnitySendMessage(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 378
    return-void
.end method
