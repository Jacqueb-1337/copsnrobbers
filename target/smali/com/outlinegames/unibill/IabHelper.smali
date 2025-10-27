.class public Lcom/outlinegames/unibill/IabHelper;
.super Ljava/lang/Object;
.source "IabHelper.java"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;,
        Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;,
        Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;,
        Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;,
        Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;
    }
.end annotation


# static fields
.field public static final BILLING_RESPONSE_RESULT_BILLING_UNAVAILABLE:I = 0x3

.field public static final BILLING_RESPONSE_RESULT_DEVELOPER_ERROR:I = 0x5

.field public static final BILLING_RESPONSE_RESULT_ERROR:I = 0x6

.field public static final BILLING_RESPONSE_RESULT_ITEM_ALREADY_OWNED:I = 0x7

.field public static final BILLING_RESPONSE_RESULT_ITEM_NOT_OWNED:I = 0x8

.field public static final BILLING_RESPONSE_RESULT_ITEM_UNAVAILABLE:I = 0x4

.field public static final BILLING_RESPONSE_RESULT_OK:I = 0x0

.field public static final BILLING_RESPONSE_RESULT_SERVICE_UNAVAILABLE:I = 0x2

.field public static final BILLING_RESPONSE_RESULT_USER_CANCELED:I = 0x1

.field public static final GET_SKU_DETAILS_ITEM_LIST:Ljava/lang/String; = "ITEM_ID_LIST"

.field public static final GET_SKU_DETAILS_ITEM_TYPE_LIST:Ljava/lang/String; = "ITEM_TYPE_LIST"

.field public static final IABHELPER_BAD_RESPONSE:I = -0x3ea

.field public static final IABHELPER_ERROR_BASE:I = -0x3e8

.field public static final IABHELPER_INVALID_CONSUMPTION:I = -0x3f2

.field public static final IABHELPER_MISSING_TOKEN:I = -0x3ef

.field public static final IABHELPER_REMOTE_EXCEPTION:I = -0x3e9

.field public static final IABHELPER_SEND_INTENT_FAILED:I = -0x3ec

.field public static final IABHELPER_SUBSCRIPTIONS_NOT_AVAILABLE:I = -0x3f1

.field public static final IABHELPER_UNKNOWN_ERROR:I = -0x3f0

.field public static final IABHELPER_UNKNOWN_PURCHASE_RESPONSE:I = -0x3ee

.field public static final IABHELPER_USER_CANCELLED:I = -0x3ed

.field public static final IABHELPER_VERIFICATION_FAILED:I = -0x3eb

.field public static final INAPP_CONTINUATION_TOKEN:Ljava/lang/String; = "INAPP_CONTINUATION_TOKEN"

.field public static final ITEM_TYPE_INAPP:Ljava/lang/String; = "inapp"

.field public static final ITEM_TYPE_SUBS:Ljava/lang/String; = "subs"

.field public static final RESPONSE_BUY_INTENT:Ljava/lang/String; = "BUY_INTENT"

.field public static final RESPONSE_CODE:Ljava/lang/String; = "RESPONSE_CODE"

.field public static final RESPONSE_GET_SKU_DETAILS_LIST:Ljava/lang/String; = "DETAILS_LIST"

.field public static final RESPONSE_INAPP_ITEM_LIST:Ljava/lang/String; = "INAPP_PURCHASE_ITEM_LIST"

.field public static final RESPONSE_INAPP_PURCHASE_DATA:Ljava/lang/String; = "INAPP_PURCHASE_DATA"

.field public static final RESPONSE_INAPP_PURCHASE_DATA_LIST:Ljava/lang/String; = "INAPP_PURCHASE_DATA_LIST"

.field public static final RESPONSE_INAPP_SIGNATURE:Ljava/lang/String; = "INAPP_DATA_SIGNATURE"

.field public static final RESPONSE_INAPP_SIGNATURE_LIST:Ljava/lang/String; = "INAPP_DATA_SIGNATURE_LIST"

.field private static serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;


# instance fields
.field private inv:Lcom/outlinegames/unibill/Inventory;

.field volatile mAsyncInProgress:Z

.field mAsyncOperation:Ljava/lang/String;

.field mContext:Landroid/content/Context;

.field mDebugLog:Z

.field mDebugTag:Ljava/lang/String;

.field volatile mDisposed:Z

.field mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

.field mPurchasingItemType:Ljava/lang/String;

.field mRequestCode:I

.field volatile mSetupDone:Z

.field mSignatureBase64:Ljava/lang/String;

.field mSubscriptionsSupported:Z


# direct methods
.method public constructor <init>(Landroid/content/Context;Ljava/lang/String;)V
    .locals 2
    .param p1, "ctx"    # Landroid/content/Context;
    .param p2, "base64PublicKey"    # Ljava/lang/String;

    .prologue
    const/4 v1, 0x0

    .line 164
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 71
    iput-boolean v1, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugLog:Z

    .line 72
    const-string v0, "IabHelper"

    iput-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugTag:Ljava/lang/String;

    .line 75
    iput-boolean v1, p0, Lcom/outlinegames/unibill/IabHelper;->mSetupDone:Z

    .line 79
    iput-boolean v1, p0, Lcom/outlinegames/unibill/IabHelper;->mDisposed:Z

    .line 82
    iput-boolean v1, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    .line 86
    iput-boolean v1, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    .line 90
    const-string v0, ""

    iput-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncOperation:Ljava/lang/String;

    .line 105
    const/4 v0, 0x0

    iput-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSignatureBase64:Ljava/lang/String;

    .line 165
    invoke-virtual {p1}, Landroid/content/Context;->getApplicationContext()Landroid/content/Context;

    move-result-object v0

    iput-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    .line 166
    new-instance v0, Lcom/outlinegames/unibill/BillingServiceManager;

    iget-object v1, p0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-direct {v0, v1}, Lcom/outlinegames/unibill/BillingServiceManager;-><init>(Landroid/content/Context;)V

    sput-object v0, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    .line 167
    iput-object p2, p0, Lcom/outlinegames/unibill/IabHelper;->mSignatureBase64:Ljava/lang/String;

    .line 168
    const-string v0, "IAB helper created."

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 169
    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/IabHelper;Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;Lcom/android/vending/billing/IInAppBillingService;)V
    .locals 0

    .prologue
    .line 229
    invoke-direct {p0, p1, p2}, Lcom/outlinegames/unibill/IabHelper;->finishSetup(Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;Lcom/android/vending/billing/IInAppBillingService;)V

    return-void
.end method

.method static synthetic access$1(Lcom/outlinegames/unibill/IabHelper;)Lcom/outlinegames/unibill/Inventory;
    .locals 1

    .prologue
    .line 108
    iget-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    return-object v0
.end method

.method private finishSetup(Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;Lcom/android/vending/billing/IInAppBillingService;)V
    .locals 7
    .param p1, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;
    .param p2, "service"    # Lcom/android/vending/billing/IInAppBillingService;

    .prologue
    .line 230
    iget-object v4, p0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-virtual {v4}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v2

    .line 232
    .local v2, "packageName":Ljava/lang/String;
    :try_start_0
    const-string v4, "Checking for in-app billing 3 support."

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 235
    const/4 v4, 0x3

    const-string v5, "inapp"

    invoke-interface {p2, v4, v2, v5}, Lcom/android/vending/billing/IInAppBillingService;->isBillingSupported(ILjava/lang/String;Ljava/lang/String;)I
    :try_end_0
    .catch Landroid/os/RemoteException; {:try_start_0 .. :try_end_0} :catch_1

    move-result v3

    .line 236
    .local v3, "response":I
    if-eqz v3, :cond_3

    .line 237
    if-eqz p1, :cond_0

    .line 239
    :try_start_1
    new-instance v4, Lcom/outlinegames/unibill/IabResult;

    .line 240
    const-string v5, "Error checking for billing v3 support."

    invoke-direct {v4, v3, v5}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 239
    invoke-interface {p1, v4}, Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;->onIabSetupFinished(Lcom/outlinegames/unibill/IabResult;)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0
    .catch Landroid/os/RemoteException; {:try_start_1 .. :try_end_1} :catch_1

    .line 246
    :cond_0
    :goto_0
    const/4 v4, 0x0

    :try_start_2
    iput-boolean v4, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    .line 283
    .end local v3    # "response":I
    :cond_1
    :goto_1
    return-void

    .line 241
    .restart local v3    # "response":I
    :catch_0
    move-exception v0

    .line 242
    .local v0, "e":Ljava/lang/Exception;
    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V
    :try_end_2
    .catch Landroid/os/RemoteException; {:try_start_2 .. :try_end_2} :catch_1

    goto :goto_0

    .line 263
    .end local v0    # "e":Ljava/lang/Exception;
    .end local v3    # "response":I
    :catch_1
    move-exception v0

    .line 264
    .local v0, "e":Landroid/os/RemoteException;
    if-eqz p1, :cond_2

    .line 266
    :try_start_3
    new-instance v4, Lcom/outlinegames/unibill/IabResult;

    const/16 v5, -0x3e9

    .line 267
    const-string v6, "RemoteException while setting up in-app billing."

    invoke-direct {v4, v5, v6}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 266
    invoke-interface {p1, v4}, Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;->onIabSetupFinished(Lcom/outlinegames/unibill/IabResult;)V
    :try_end_3
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_3

    .line 272
    :cond_2
    :goto_2
    invoke-virtual {v0}, Landroid/os/RemoteException;->printStackTrace()V

    goto :goto_1

    .line 249
    .end local v0    # "e":Landroid/os/RemoteException;
    .restart local v3    # "response":I
    :cond_3
    :try_start_4
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "In-app billing version 3 supported for "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 252
    const/4 v4, 0x3

    const-string v5, "subs"

    invoke-interface {p2, v4, v2, v5}, Lcom/android/vending/billing/IInAppBillingService;->isBillingSupported(ILjava/lang/String;Ljava/lang/String;)I

    move-result v3

    .line 253
    if-nez v3, :cond_4

    .line 254
    const-string v4, "Subscriptions AVAILABLE."

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 255
    const/4 v4, 0x1

    iput-boolean v4, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    .line 261
    :goto_3
    const/4 v4, 0x1

    iput-boolean v4, p0, Lcom/outlinegames/unibill/IabHelper;->mSetupDone:Z
    :try_end_4
    .catch Landroid/os/RemoteException; {:try_start_4 .. :try_end_4} :catch_1

    .line 276
    if-eqz p1, :cond_1

    .line 278
    :try_start_5
    new-instance v4, Lcom/outlinegames/unibill/IabResult;

    const/4 v5, 0x0

    const-string v6, "Setup successful."

    invoke-direct {v4, v5, v6}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    invoke-interface {p1, v4}, Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;->onIabSetupFinished(Lcom/outlinegames/unibill/IabResult;)V
    :try_end_5
    .catch Ljava/lang/Exception; {:try_start_5 .. :try_end_5} :catch_2

    goto :goto_1

    .line 279
    :catch_2
    move-exception v0

    .line 280
    .local v0, "e":Ljava/lang/Exception;
    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_1

    .line 258
    .end local v0    # "e":Ljava/lang/Exception;
    :cond_4
    :try_start_6
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Subscriptions NOT AVAILABLE. Response: "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V
    :try_end_6
    .catch Landroid/os/RemoteException; {:try_start_6 .. :try_end_6} :catch_1

    goto :goto_3

    .line 268
    .end local v3    # "response":I
    .local v0, "e":Landroid/os/RemoteException;
    :catch_3
    move-exception v1

    .line 269
    .local v1, "e1":Ljava/lang/Exception;
    invoke-virtual {v1}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_2
.end method

.method public static getResponseDesc(I)Ljava/lang/String;
    .locals 5
    .param p0, "code"    # I

    .prologue
    .line 760
    const-string v3, "0:OK/1:User Canceled/2:Unknown/3:Billing Unavailable/4:Item unavailable/5:Developer Error/6:Error/7:Item Already Owned/8:Item not owned"

    .line 763
    const-string v4, "/"

    invoke-virtual {v3, v4}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;

    move-result-object v0

    .line 764
    .local v0, "iab_msgs":[Ljava/lang/String;
    const-string v3, "0:OK/-1001:Remote exception during initialization/-1002:Bad response received/-1003:Purchase signature verification failed/-1004:Send intent failed/-1005:User cancelled/-1006:Unknown purchase response/-1007:Missing token/-1008:Unknown error/-1009:Subscriptions not available/-1010:Invalid consumption attempt"

    .line 773
    const-string v4, "/"

    invoke-virtual {v3, v4}, Ljava/lang/String;->split(Ljava/lang/String;)[Ljava/lang/String;

    move-result-object v1

    .line 775
    .local v1, "iabhelper_msgs":[Ljava/lang/String;
    const/16 v3, -0x3e8

    if-gt p0, v3, :cond_1

    .line 776
    rsub-int v2, p0, -0x3e8

    .line 777
    .local v2, "index":I
    if-ltz v2, :cond_0

    array-length v3, v1

    if-ge v2, v3, :cond_0

    aget-object v3, v1, v2

    .line 783
    .end local v2    # "index":I
    :goto_0
    return-object v3

    .line 778
    .restart local v2    # "index":I
    :cond_0
    new-instance v3, Ljava/lang/StringBuilder;

    invoke-static {p0}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    const-string v4, ":Unknown IAB Helper Error"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    goto :goto_0

    .line 780
    .end local v2    # "index":I
    :cond_1
    if-ltz p0, :cond_2

    array-length v3, v0

    if-lt p0, v3, :cond_3

    .line 781
    :cond_2
    new-instance v3, Ljava/lang/StringBuilder;

    invoke-static {p0}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Ljava/lang/String;->valueOf(Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    const-string v4, ":Unknown"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    goto :goto_0

    .line 783
    :cond_3
    aget-object v3, v0, p0

    goto :goto_0
.end method


# virtual methods
.method checkSetupDone(Ljava/lang/String;)V
    .locals 3
    .param p1, "operation"    # Ljava/lang/String;

    .prologue
    .line 789
    iget-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSetupDone:Z

    if-nez v0, :cond_0

    .line 790
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Illegal state for operation ("

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "): IAB helper is not set up."

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 791
    new-instance v0, Ljava/lang/IllegalStateException;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "IAB helper is not set up. Can\'t perform operation: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 793
    :cond_0
    return-void
.end method

.method consume(Lcom/outlinegames/unibill/Purchase;Lcom/android/vending/billing/IInAppBillingService;)V
    .locals 8
    .param p1, "itemInfo"    # Lcom/outlinegames/unibill/Purchase;
    .param p2, "service"    # Lcom/android/vending/billing/IInAppBillingService;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lcom/outlinegames/unibill/IabException;
        }
    .end annotation

    .prologue
    .line 670
    iget-object v4, p1, Lcom/outlinegames/unibill/Purchase;->mItemType:Ljava/lang/String;

    const-string v5, "inapp"

    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-nez v4, :cond_0

    .line 671
    new-instance v4, Lcom/outlinegames/unibill/IabException;

    const/16 v5, -0x3f2

    .line 672
    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "Items of type \'"

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v7, p1, Lcom/outlinegames/unibill/Purchase;->mItemType:Ljava/lang/String;

    invoke-virtual {v6, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    const-string v7, "\' can\'t be consumed."

    invoke-virtual {v6, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    invoke-virtual {v6}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v6

    .line 671
    invoke-direct {v4, v5, v6}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v4

    .line 676
    :cond_0
    :try_start_0
    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getToken()Ljava/lang/String;

    move-result-object v3

    .line 677
    .local v3, "token":Ljava/lang/String;
    invoke-virtual {p1}, Lcom/outlinegames/unibill/Purchase;->getSku()Ljava/lang/String;

    move-result-object v2

    .line 678
    .local v2, "sku":Ljava/lang/String;
    if-eqz v3, :cond_1

    const-string v4, ""

    invoke-virtual {v3, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v4

    if-eqz v4, :cond_2

    .line 679
    :cond_1
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Can\'t consume "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, ". No token."

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 680
    new-instance v4, Lcom/outlinegames/unibill/IabException;

    const/16 v5, -0x3ef

    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "PurchaseInfo is missing token for sku: "

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    .line 681
    invoke-virtual {v6, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    const-string v7, " "

    invoke-virtual {v6, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    invoke-virtual {v6, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v6

    invoke-virtual {v6}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v6

    .line 680
    invoke-direct {v4, v5, v6}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v4
    :try_end_0
    .catch Landroid/os/RemoteException; {:try_start_0 .. :try_end_0} :catch_0

    .line 694
    .end local v2    # "sku":Ljava/lang/String;
    .end local v3    # "token":Ljava/lang/String;
    :catch_0
    move-exception v0

    .line 695
    .local v0, "e":Landroid/os/RemoteException;
    new-instance v4, Lcom/outlinegames/unibill/IabException;

    const/16 v5, -0x3e9

    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "Remote exception while consuming. PurchaseInfo: "

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v6, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v6

    invoke-virtual {v6}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v6

    invoke-direct {v4, v5, v6, v0}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;Ljava/lang/Exception;)V

    throw v4

    .line 684
    .end local v0    # "e":Landroid/os/RemoteException;
    .restart local v2    # "sku":Ljava/lang/String;
    .restart local v3    # "token":Ljava/lang/String;
    :cond_2
    :try_start_1
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Consuming sku: "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, ", token: "

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 685
    const/4 v4, 0x3

    iget-object v5, p0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-virtual {v5}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v5

    invoke-interface {p2, v4, v5, v3}, Lcom/android/vending/billing/IInAppBillingService;->consumePurchase(ILjava/lang/String;Ljava/lang/String;)I

    move-result v1

    .line 686
    .local v1, "response":I
    if-nez v1, :cond_3

    .line 687
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Successfully consumed sku: "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 697
    return-void

    .line 690
    :cond_3
    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "Error consuming consuming sku "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, ". "

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-static {v1}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0, v4}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 691
    new-instance v4, Lcom/outlinegames/unibill/IabException;

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "Error consuming sku "

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-direct {v4, v1, v5}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v4
    :try_end_1
    .catch Landroid/os/RemoteException; {:try_start_1 .. :try_end_1} :catch_0
.end method

.method public consumeAsync(Lcom/outlinegames/unibill/Purchase;JLcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;)V
    .locals 6
    .param p1, "purchase"    # Lcom/outlinegames/unibill/Purchase;
    .param p2, "delay"    # J
    .param p4, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;

    .prologue
    .line 736
    const-string v0, "consume"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 737
    new-instance v1, Ljava/util/ArrayList;

    invoke-direct {v1}, Ljava/util/ArrayList;-><init>()V

    .line 738
    .local v1, "purchases":Ljava/util/List;, "Ljava/util/List<Lcom/outlinegames/unibill/Purchase;>;"
    invoke-interface {v1, p1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    .line 739
    const/4 v3, 0x0

    move-object v0, p0

    move-object v2, p4

    move-wide v4, p2

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->consumeAsyncInternal(Ljava/util/List;Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;J)V

    .line 740
    return-void
.end method

.method public consumeAsync(Ljava/util/List;Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;)V
    .locals 6
    .param p2, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/List",
            "<",
            "Lcom/outlinegames/unibill/Purchase;",
            ">;",
            "Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;",
            ")V"
        }
    .end annotation

    .prologue
    .line 748
    .local p1, "purchases":Ljava/util/List;, "Ljava/util/List<Lcom/outlinegames/unibill/Purchase;>;"
    const-string v0, "consume"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 749
    const/4 v2, 0x0

    const-wide/16 v4, 0x0

    move-object v0, p0

    move-object v1, p1

    move-object v3, p2

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->consumeAsyncInternal(Ljava/util/List;Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;J)V

    .line 750
    return-void
.end method

.method consumeAsyncInternal(Ljava/util/List;Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;J)V
    .locals 8
    .param p2, "singleListener"    # Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;
    .param p3, "multiListener"    # Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;
    .param p4, "delay"    # J
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/List",
            "<",
            "Lcom/outlinegames/unibill/Purchase;",
            ">;",
            "Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;",
            "Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;",
            "J)V"
        }
    .end annotation

    .prologue
    .line 966
    .local p1, "purchases":Ljava/util/List;, "Ljava/util/List<Lcom/outlinegames/unibill/Purchase;>;"
    const-string v0, "consume"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->flagStartAsync(Ljava/lang/String;)V

    .line 967
    sget-object v7, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    new-instance v0, Lcom/outlinegames/unibill/IabHelper$4;

    move-object v1, p0

    move-wide v2, p4

    move-object v4, p1

    move-object v5, p2

    move-object v6, p3

    invoke-direct/range {v0 .. v6}, Lcom/outlinegames/unibill/IabHelper$4;-><init>(Lcom/outlinegames/unibill/IabHelper;JLjava/util/List;Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;Lcom/outlinegames/unibill/IabHelper$OnConsumeMultiFinishedListener;)V

    invoke-virtual {v7, v0}, Lcom/outlinegames/unibill/BillingServiceManager;->workWith(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V

    .line 999
    return-void
.end method

.method public dispose()V
    .locals 1

    .prologue
    .line 292
    const-string v0, "Disposing."

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 293
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSetupDone:Z

    .line 294
    sget-object v0, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    invoke-virtual {v0}, Lcom/outlinegames/unibill/BillingServiceManager;->dispose()V

    .line 295
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mDisposed:Z

    .line 296
    return-void
.end method

.method public enableDebugLogging(Z)V
    .locals 0
    .param p1, "enable"    # Z

    .prologue
    .line 180
    iput-boolean p1, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugLog:Z

    .line 181
    return-void
.end method

.method public enableDebugLogging(ZLjava/lang/String;)V
    .locals 0
    .param p1, "enable"    # Z
    .param p2, "tag"    # Ljava/lang/String;

    .prologue
    .line 175
    iput-boolean p1, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugLog:Z

    .line 176
    iput-object p2, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugTag:Ljava/lang/String;

    .line 177
    return-void
.end method

.method flagEndAsync()V
    .locals 2

    .prologue
    .line 836
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Ending async operation: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncOperation:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 837
    const-string v0, ""

    iput-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncOperation:Ljava/lang/String;

    .line 838
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    .line 839
    return-void
.end method

.method flagStartAsync(Ljava/lang/String;)V
    .locals 3
    .param p1, "operation"    # Ljava/lang/String;

    .prologue
    .line 828
    iget-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    if-eqz v0, :cond_0

    new-instance v0, Ljava/lang/IllegalStateException;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Can\'t start async operation ("

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    .line 829
    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, ") because another async operation("

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    iget-object v2, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncOperation:Ljava/lang/String;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, ") is in progress."

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 830
    :cond_0
    iput-object p1, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncOperation:Ljava/lang/String;

    .line 831
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mAsyncInProgress:Z

    .line 832
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Starting async operation: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 833
    return-void
.end method

.method getResponseCodeFromBundle(Landroid/os/Bundle;)I
    .locals 4
    .param p1, "b"    # Landroid/os/Bundle;

    .prologue
    .line 797
    const-string v1, "RESPONSE_CODE"

    invoke-virtual {p1, v1}, Landroid/os/Bundle;->get(Ljava/lang/String;)Ljava/lang/Object;

    move-result-object v0

    .line 798
    .local v0, "o":Ljava/lang/Object;
    if-nez v0, :cond_0

    .line 799
    const-string v1, "Bundle with null response code, assuming OK (known issue)"

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 800
    const/4 v1, 0x0

    .line 803
    .end local v0    # "o":Ljava/lang/Object;
    :goto_0
    return v1

    .line 802
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_0
    instance-of v1, v0, Ljava/lang/Integer;

    if-eqz v1, :cond_1

    check-cast v0, Ljava/lang/Integer;

    .end local v0    # "o":Ljava/lang/Object;
    invoke-virtual {v0}, Ljava/lang/Integer;->intValue()I

    move-result v1

    goto :goto_0

    .line 803
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_1
    instance-of v1, v0, Ljava/lang/Long;

    if-eqz v1, :cond_2

    check-cast v0, Ljava/lang/Long;

    .end local v0    # "o":Ljava/lang/Object;
    invoke-virtual {v0}, Ljava/lang/Long;->longValue()J

    move-result-wide v2

    long-to-int v1, v2

    goto :goto_0

    .line 805
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_2
    const-string v1, "Unexpected type for bundle response code."

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 806
    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 807
    new-instance v1, Ljava/lang/RuntimeException;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unexpected type for bundle response code: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    throw v1
.end method

.method getResponseCodeFromIntent(Landroid/content/Intent;)I
    .locals 4
    .param p1, "i"    # Landroid/content/Intent;

    .prologue
    .line 813
    invoke-virtual {p1}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v1

    const-string v2, "RESPONSE_CODE"

    invoke-virtual {v1, v2}, Landroid/os/Bundle;->get(Ljava/lang/String;)Ljava/lang/Object;

    move-result-object v0

    .line 814
    .local v0, "o":Ljava/lang/Object;
    if-nez v0, :cond_0

    .line 815
    const-string v1, "Intent with no response code, assuming OK (known issue)"

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 816
    const/4 v1, 0x0

    .line 819
    .end local v0    # "o":Ljava/lang/Object;
    :goto_0
    return v1

    .line 818
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_0
    instance-of v1, v0, Ljava/lang/Integer;

    if-eqz v1, :cond_1

    check-cast v0, Ljava/lang/Integer;

    .end local v0    # "o":Ljava/lang/Object;
    invoke-virtual {v0}, Ljava/lang/Integer;->intValue()I

    move-result v1

    goto :goto_0

    .line 819
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_1
    instance-of v1, v0, Ljava/lang/Long;

    if-eqz v1, :cond_2

    check-cast v0, Ljava/lang/Long;

    .end local v0    # "o":Ljava/lang/Object;
    invoke-virtual {v0}, Ljava/lang/Long;->longValue()J

    move-result-wide v2

    long-to-int v1, v2

    goto :goto_0

    .line 821
    .restart local v0    # "o":Ljava/lang/Object;
    :cond_2
    const-string v1, "Unexpected type for intent response code."

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 822
    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p0, v1}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 823
    new-instance v1, Ljava/lang/RuntimeException;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unexpected type for intent response code: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    throw v1
.end method

.method public handleActivityResult(IILandroid/content/Intent;)Z
    .locals 12
    .param p1, "requestCode"    # I
    .param p2, "resultCode"    # I
    .param p3, "data"    # Landroid/content/Intent;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 449
    iget v8, p0, Lcom/outlinegames/unibill/IabHelper;->mRequestCode:I

    if-eq p1, v8, :cond_0

    const/4 v8, 0x0

    .line 527
    :goto_0
    return v8

    .line 451
    :cond_0
    const-string v8, "handleActivityResult"

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 454
    invoke-virtual {p0}, Lcom/outlinegames/unibill/IabHelper;->flagEndAsync()V

    .line 456
    if-nez p3, :cond_2

    .line 457
    const-string v8, "Null data in IAB activity result."

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 458
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3ea

    const-string v9, "Null data in IAB result"

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 459
    .local v6, "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_1

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 460
    :cond_1
    const/4 v8, 0x1

    goto :goto_0

    .line 463
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_2
    invoke-virtual {p0, p3}, Lcom/outlinegames/unibill/IabHelper;->getResponseCodeFromIntent(Landroid/content/Intent;)I

    move-result v5

    .line 464
    .local v5, "responseCode":I
    const-string v8, "INAPP_PURCHASE_DATA"

    invoke-virtual {p3, v8}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    .line 465
    .local v4, "purchaseData":Ljava/lang/String;
    const-string v8, "INAPP_DATA_SIGNATURE"

    invoke-virtual {p3, v8}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    .line 467
    .local v0, "dataSignature":Ljava/lang/String;
    const/4 v8, -0x1

    if-ne p2, v8, :cond_a

    if-nez v5, :cond_a

    .line 468
    const-string v8, "Successful resultcode from purchase activity."

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 469
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Purchase data: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v8, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 470
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Data signature: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v8, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 471
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Extras: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p3}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 472
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Expected item type: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v9, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchasingItemType:Ljava/lang/String;

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 474
    if-eqz v4, :cond_3

    if-nez v0, :cond_5

    .line 475
    :cond_3
    const-string v8, "BUG: either purchaseData or dataSignature is null."

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 476
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Extras: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p3}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v9

    invoke-virtual {v9}, Landroid/os/Bundle;->toString()Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 477
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3f0

    const-string v9, "IAB returned null purchaseData or dataSignature"

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 478
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_4

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 479
    :cond_4
    const/4 v8, 0x1

    goto/16 :goto_0

    .line 482
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_5
    const/4 v2, 0x0

    .line 484
    .local v2, "purchase":Lcom/outlinegames/unibill/Purchase;
    :try_start_0
    new-instance v3, Lcom/outlinegames/unibill/Purchase;

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchasingItemType:Ljava/lang/String;

    invoke-direct {v3, v8, v4, v0}, Lcom/outlinegames/unibill/Purchase;-><init>(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    .line 485
    .end local v2    # "purchase":Lcom/outlinegames/unibill/Purchase;
    .local v3, "purchase":Lcom/outlinegames/unibill/Purchase;
    :try_start_1
    invoke-virtual {v3}, Lcom/outlinegames/unibill/Purchase;->getSku()Ljava/lang/String;

    move-result-object v7

    .line 488
    .local v7, "sku":Ljava/lang/String;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mSignatureBase64:Ljava/lang/String;

    invoke-static {v8, v4, v0}, Lcom/outlinegames/unibill/Security;->verifyPurchase(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Z

    move-result v8

    if-nez v8, :cond_7

    .line 489
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Purchase signature verification FAILED for sku "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v8, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 490
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3eb

    new-instance v9, Ljava/lang/StringBuilder;

    const-string v10, "Signature verification failed for sku "

    invoke-direct {v9, v10}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v9, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v9

    invoke-virtual {v9}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v9

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 491
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_6

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    invoke-interface {v8, v6, v3}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 492
    :cond_6
    const/4 v8, 0x1

    goto/16 :goto_0

    .line 494
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_7
    const-string v8, "Purchase signature successfully verified."

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_1

    .line 504
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_8

    .line 505
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    new-instance v9, Lcom/outlinegames/unibill/IabResult;

    const/4 v10, 0x0

    const-string v11, "Success"

    invoke-direct {v9, v10, v11}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    invoke-interface {v8, v9, v3}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 527
    .end local v3    # "purchase":Lcom/outlinegames/unibill/Purchase;
    .end local v7    # "sku":Ljava/lang/String;
    :cond_8
    :goto_1
    const/4 v8, 0x1

    goto/16 :goto_0

    .line 496
    .restart local v2    # "purchase":Lcom/outlinegames/unibill/Purchase;
    :catch_0
    move-exception v1

    .line 497
    .local v1, "e":Lorg/json/JSONException;
    :goto_2
    const-string v8, "Failed to parse purchase data."

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 498
    invoke-virtual {v1}, Lorg/json/JSONException;->printStackTrace()V

    .line 499
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3ea

    const-string v9, "Failed to parse purchase data."

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 500
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_9

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 501
    :cond_9
    const/4 v8, 0x1

    goto/16 :goto_0

    .line 508
    .end local v1    # "e":Lorg/json/JSONException;
    .end local v2    # "purchase":Lcom/outlinegames/unibill/Purchase;
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_a
    const/4 v8, -0x1

    if-ne p2, v8, :cond_b

    .line 510
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Result code was OK but in-app billing response was not OK: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {v5}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 511
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_8

    .line 512
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const-string v8, "Problem purchashing item."

    invoke-direct {v6, v5, v8}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 513
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    goto :goto_1

    .line 516
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_b
    if-nez p2, :cond_c

    .line 517
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Purchase canceled - Response: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {v5}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 518
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3ed

    const-string v9, "User canceled."

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 519
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_8

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    goto :goto_1

    .line 522
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    :cond_c
    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "Purchase failed. Result code: "

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {p2}, Ljava/lang/Integer;->toString(I)Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    .line 523
    const-string v9, ". Response: "

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-static {v5}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v9

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    .line 522
    invoke-virtual {p0, v8}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 524
    new-instance v6, Lcom/outlinegames/unibill/IabResult;

    const/16 v8, -0x3ee

    const-string v9, "Unknown purchase response."

    invoke-direct {v6, v8, v9}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 525
    .restart local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    if-eqz v8, :cond_8

    iget-object v8, p0, Lcom/outlinegames/unibill/IabHelper;->mPurchaseListener:Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;

    const/4 v9, 0x0

    invoke-interface {v8, v6, v9}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    goto/16 :goto_1

    .line 496
    .end local v6    # "result":Lcom/outlinegames/unibill/IabResult;
    .restart local v3    # "purchase":Lcom/outlinegames/unibill/Purchase;
    :catch_1
    move-exception v1

    move-object v2, v3

    .end local v3    # "purchase":Lcom/outlinegames/unibill/Purchase;
    .restart local v2    # "purchase":Lcom/outlinegames/unibill/Purchase;
    goto/16 :goto_2
.end method

.method public launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;)V
    .locals 6
    .param p1, "act"    # Landroid/app/Activity;
    .param p2, "sku"    # Ljava/lang/String;
    .param p3, "requestCode"    # I
    .param p4, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 326
    const-string v5, ""

    move-object v0, p0

    move-object v1, p1

    move-object v2, p2

    move v3, p3

    move-object v4, p4

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V

    .line 327
    return-void
.end method

.method public launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V
    .locals 7
    .param p1, "act"    # Landroid/app/Activity;
    .param p2, "sku"    # Ljava/lang/String;
    .param p3, "requestCode"    # I
    .param p4, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;
    .param p5, "extraData"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 331
    const-string v3, "inapp"

    move-object v0, p0

    move-object v1, p1

    move-object v2, p2

    move v4, p3

    move-object v5, p4

    move-object v6, p5

    invoke-virtual/range {v0 .. v6}, Lcom/outlinegames/unibill/IabHelper;->launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V

    .line 332
    return-void
.end method

.method public launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V
    .locals 10
    .param p1, "act"    # Landroid/app/Activity;
    .param p2, "sku"    # Ljava/lang/String;
    .param p3, "itemType"    # Ljava/lang/String;
    .param p4, "requestCode"    # I
    .param p5, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;
    .param p6, "extraData"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 365
    const-string v0, "launchPurchaseFlow"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 366
    const-string v0, "launchPurchaseFlow"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->flagStartAsync(Ljava/lang/String;)V

    .line 368
    const-string v0, "subs"

    invoke-virtual {p3, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    if-nez v0, :cond_1

    .line 369
    new-instance v8, Lcom/outlinegames/unibill/IabResult;

    const/16 v0, -0x3f1

    .line 370
    const-string v1, "Subscriptions are not available."

    .line 369
    invoke-direct {v8, v0, v1}, Lcom/outlinegames/unibill/IabResult;-><init>(ILjava/lang/String;)V

    .line 371
    .local v8, "r":Lcom/outlinegames/unibill/IabResult;
    if-eqz p5, :cond_0

    const/4 v0, 0x0

    invoke-interface {p5, v8, v0}, Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;->onIabPurchaseFinished(Lcom/outlinegames/unibill/IabResult;Lcom/outlinegames/unibill/Purchase;)V

    .line 431
    .end local v8    # "r":Lcom/outlinegames/unibill/IabResult;
    :cond_0
    :goto_0
    return-void

    .line 374
    :cond_1
    sget-object v9, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    new-instance v0, Lcom/outlinegames/unibill/IabHelper$2;

    move-object v1, p0

    move-object v2, p2

    move-object v3, p3

    move-object/from16 v4, p6

    move-object v5, p5

    move v6, p4

    move-object v7, p1

    invoke-direct/range {v0 .. v7}, Lcom/outlinegames/unibill/IabHelper$2;-><init>(Lcom/outlinegames/unibill/IabHelper;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;ILandroid/app/Activity;)V

    invoke-virtual {v9, v0}, Lcom/outlinegames/unibill/BillingServiceManager;->workWith(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V

    goto :goto_0
.end method

.method public launchSubscriptionPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;)V
    .locals 6
    .param p1, "act"    # Landroid/app/Activity;
    .param p2, "sku"    # Ljava/lang/String;
    .param p3, "requestCode"    # I
    .param p4, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 336
    const-string v5, ""

    move-object v0, p0

    move-object v1, p1

    move-object v2, p2

    move v3, p3

    move-object v4, p4

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->launchSubscriptionPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V

    .line 337
    return-void
.end method

.method public launchSubscriptionPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V
    .locals 7
    .param p1, "act"    # Landroid/app/Activity;
    .param p2, "sku"    # Ljava/lang/String;
    .param p3, "requestCode"    # I
    .param p4, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;
    .param p5, "extraData"    # Ljava/lang/String;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 341
    const-string v3, "subs"

    move-object v0, p0

    move-object v1, p1

    move-object v2, p2

    move v4, p3

    move-object v5, p4

    move-object v6, p5

    invoke-virtual/range {v0 .. v6}, Lcom/outlinegames/unibill/IabHelper;->launchPurchaseFlow(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;ILcom/outlinegames/unibill/IabHelper$OnIabPurchaseFinishedListener;Ljava/lang/String;)V

    .line 342
    return-void
.end method

.method logDebug(Ljava/lang/String;)V
    .locals 1
    .param p1, "msg"    # Ljava/lang/String;

    .prologue
    .line 1002
    const-string v0, "Unibill"

    invoke-static {v0, p1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 1003
    return-void
.end method

.method logError(Ljava/lang/String;)V
    .locals 3
    .param p1, "msg"    # Ljava/lang/String;

    .prologue
    .line 1006
    iget-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugTag:Ljava/lang/String;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "In-app billing error: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Landroid/util/Log;->e(Ljava/lang/String;Ljava/lang/String;)I

    .line 1007
    return-void
.end method

.method logWarn(Ljava/lang/String;)V
    .locals 3
    .param p1, "msg"    # Ljava/lang/String;

    .prologue
    .line 1010
    iget-object v0, p0, Lcom/outlinegames/unibill/IabHelper;->mDebugTag:Ljava/lang/String;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "In-app billing warning: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;)I

    .line 1011
    return-void
.end method

.method public queryInventory(ZLjava/util/List;Lcom/android/vending/billing/IInAppBillingService;)Lcom/outlinegames/unibill/Inventory;
    .locals 1
    .param p1, "querySkuDetails"    # Z
    .param p3, "service"    # Lcom/android/vending/billing/IInAppBillingService;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(Z",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Lcom/android/vending/billing/IInAppBillingService;",
            ")",
            "Lcom/outlinegames/unibill/Inventory;"
        }
    .end annotation

    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lcom/outlinegames/unibill/IabException;
        }
    .end annotation

    .prologue
    .line 531
    .local p2, "moreSkus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    const/4 v0, 0x0

    invoke-virtual {p0, p1, p2, v0, p3}, Lcom/outlinegames/unibill/IabHelper;->queryInventory(ZLjava/util/List;Ljava/util/List;Lcom/android/vending/billing/IInAppBillingService;)Lcom/outlinegames/unibill/Inventory;

    move-result-object v0

    return-object v0
.end method

.method public queryInventory(ZLjava/util/List;Ljava/util/List;Lcom/android/vending/billing/IInAppBillingService;)Lcom/outlinegames/unibill/Inventory;
    .locals 5
    .param p1, "querySkuDetails"    # Z
    .param p4, "service"    # Lcom/android/vending/billing/IInAppBillingService;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(Z",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Lcom/android/vending/billing/IInAppBillingService;",
            ")",
            "Lcom/outlinegames/unibill/Inventory;"
        }
    .end annotation

    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lcom/outlinegames/unibill/IabException;
        }
    .end annotation

    .prologue
    .line 549
    .local p2, "moreItemSkus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    .local p3, "moreSubsSkus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    const-string v2, "queryInventory"

    invoke-virtual {p0, v2}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 551
    :try_start_0
    new-instance v2, Lcom/outlinegames/unibill/Inventory;

    invoke-direct {v2}, Lcom/outlinegames/unibill/Inventory;-><init>()V

    iput-object v2, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    .line 552
    iget-object v2, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    const-string v3, "inapp"

    invoke-virtual {p0, v2, v3, p4}, Lcom/outlinegames/unibill/IabHelper;->queryPurchases(Lcom/outlinegames/unibill/Inventory;Ljava/lang/String;Lcom/android/vending/billing/IInAppBillingService;)I

    move-result v1

    .line 553
    .local v1, "r":I
    if-eqz v1, :cond_0

    .line 554
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const-string v3, "Error refreshing inventory (querying owned items)."

    invoke-direct {v2, v1, v3}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v2
    :try_end_0
    .catch Landroid/os/RemoteException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_1

    .line 581
    .end local v1    # "r":I
    :catch_0
    move-exception v0

    .line 582
    .local v0, "e":Landroid/os/RemoteException;
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const/16 v3, -0x3e9

    const-string v4, "Remote exception while refreshing inventory."

    invoke-direct {v2, v3, v4, v0}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;Ljava/lang/Exception;)V

    throw v2

    .line 557
    .end local v0    # "e":Landroid/os/RemoteException;
    .restart local v1    # "r":I
    :cond_0
    if-eqz p1, :cond_1

    .line 558
    :try_start_1
    const-string v2, "inapp"

    iget-object v3, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    invoke-virtual {p0, v2, v3, p2, p4}, Lcom/outlinegames/unibill/IabHelper;->querySkuDetails(Ljava/lang/String;Lcom/outlinegames/unibill/Inventory;Ljava/util/List;Lcom/android/vending/billing/IInAppBillingService;)I

    move-result v1

    .line 559
    if-eqz v1, :cond_1

    .line 560
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const-string v3, "Error refreshing inventory (querying prices of items)."

    invoke-direct {v2, v1, v3}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v2
    :try_end_1
    .catch Landroid/os/RemoteException; {:try_start_1 .. :try_end_1} :catch_0
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_1

    .line 584
    .end local v1    # "r":I
    :catch_1
    move-exception v0

    .line 585
    .local v0, "e":Lorg/json/JSONException;
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const/16 v3, -0x3ea

    const-string v4, "Error parsing JSON response while refreshing inventory."

    invoke-direct {v2, v3, v4, v0}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;Ljava/lang/Exception;)V

    throw v2

    .line 565
    .end local v0    # "e":Lorg/json/JSONException;
    .restart local v1    # "r":I
    :cond_1
    :try_start_2
    iget-boolean v2, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    if-eqz v2, :cond_3

    .line 566
    iget-object v2, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    const-string v3, "subs"

    invoke-virtual {p0, v2, v3, p4}, Lcom/outlinegames/unibill/IabHelper;->queryPurchases(Lcom/outlinegames/unibill/Inventory;Ljava/lang/String;Lcom/android/vending/billing/IInAppBillingService;)I

    move-result v1

    .line 567
    if-eqz v1, :cond_2

    .line 568
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const-string v3, "Error refreshing inventory (querying owned subscriptions)."

    invoke-direct {v2, v1, v3}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v2

    .line 571
    :cond_2
    if-eqz p1, :cond_3

    .line 572
    const-string v2, "subs"

    iget-object v3, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;

    invoke-virtual {p0, v2, v3, p2, p4}, Lcom/outlinegames/unibill/IabHelper;->querySkuDetails(Ljava/lang/String;Lcom/outlinegames/unibill/Inventory;Ljava/util/List;Lcom/android/vending/billing/IInAppBillingService;)I

    move-result v1

    .line 573
    if-eqz v1, :cond_3

    .line 574
    new-instance v2, Lcom/outlinegames/unibill/IabException;

    const-string v3, "Error refreshing inventory (querying prices of subscriptions)."

    invoke-direct {v2, v1, v3}, Lcom/outlinegames/unibill/IabException;-><init>(ILjava/lang/String;)V

    throw v2

    .line 579
    :cond_3
    iget-object v2, p0, Lcom/outlinegames/unibill/IabHelper;->inv:Lcom/outlinegames/unibill/Inventory;
    :try_end_2
    .catch Landroid/os/RemoteException; {:try_start_2 .. :try_end_2} :catch_0
    .catch Lorg/json/JSONException; {:try_start_2 .. :try_end_2} :catch_1

    return-object v2
.end method

.method public queryInventoryAsync(Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;)V
    .locals 6
    .param p1, "listener"    # Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;

    .prologue
    .line 652
    const/4 v1, 0x1

    const/4 v2, 0x0

    const-wide/16 v4, 0x0

    move-object v0, p0

    move-object v3, p1

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->queryInventoryAsync(ZLjava/util/List;Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;J)V

    .line 653
    return-void
.end method

.method public queryInventoryAsync(ZLcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;)V
    .locals 6
    .param p1, "querySkuDetails"    # Z
    .param p2, "listener"    # Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;

    .prologue
    .line 656
    const/4 v2, 0x0

    const-wide/16 v4, 0x0

    move-object v0, p0

    move v1, p1

    move-object v3, p2

    invoke-virtual/range {v0 .. v5}, Lcom/outlinegames/unibill/IabHelper;->queryInventoryAsync(ZLjava/util/List;Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;J)V

    .line 657
    return-void
.end method

.method public queryInventoryAsync(ZLjava/util/List;Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;J)V
    .locals 8
    .param p1, "querySkuDetails"    # Z
    .param p3, "listener"    # Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;
    .param p4, "delay"    # J
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(Z",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;",
            "J)V"
        }
    .end annotation

    .prologue
    .line 617
    .local p2, "moreSkus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    const-string v0, "queryInventory"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->checkSetupDone(Ljava/lang/String;)V

    .line 618
    const-string v0, "refresh inventory"

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->flagStartAsync(Ljava/lang/String;)V

    .line 619
    sget-object v7, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    new-instance v0, Lcom/outlinegames/unibill/IabHelper$3;

    move-object v1, p0

    move-wide v2, p4

    move v4, p1

    move-object v5, p2

    move-object v6, p3

    invoke-direct/range {v0 .. v6}, Lcom/outlinegames/unibill/IabHelper$3;-><init>(Lcom/outlinegames/unibill/IabHelper;JZLjava/util/List;Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;)V

    invoke-virtual {v7, v0}, Lcom/outlinegames/unibill/BillingServiceManager;->workWith(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V

    .line 649
    return-void
.end method

.method queryPurchases(Lcom/outlinegames/unibill/Inventory;Ljava/lang/String;Lcom/android/vending/billing/IInAppBillingService;)I
    .locals 16
    .param p1, "inv"    # Lcom/outlinegames/unibill/Inventory;
    .param p2, "itemType"    # Ljava/lang/String;
    .param p3, "service"    # Lcom/android/vending/billing/IInAppBillingService;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;,
            Landroid/os/RemoteException;
        }
    .end annotation

    .prologue
    .line 843
    move-object/from16 v0, p0

    iget-boolean v14, v0, Lcom/outlinegames/unibill/IabHelper;->mDisposed:Z

    if-eqz v14, :cond_0

    .line 844
    const-string v14, "queryPurchases - Biller disposed. Returning..."

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 845
    const/4 v9, 0x0

    .line 906
    :goto_0
    return v9

    .line 848
    :cond_0
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Querying owned items, item type: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    move-object/from16 v0, p2

    invoke-virtual {v14, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 849
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Package name: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    move-object/from16 v0, p0

    iget-object v15, v0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-virtual {v15}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v15

    invoke-virtual {v14, v15}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 850
    const/4 v13, 0x0

    .line 852
    .local v13, "verificationFailed":Z
    const/4 v2, 0x0

    .line 855
    .local v2, "continueToken":Ljava/lang/String;
    :cond_1
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Calling getPurchases with continuation token: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 856
    const/4 v14, 0x3

    move-object/from16 v0, p0

    iget-object v15, v0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-virtual {v15}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v15

    move-object/from16 v0, p3

    move-object/from16 v1, p2

    invoke-interface {v0, v14, v15, v1, v2}, Lcom/android/vending/billing/IInAppBillingService;->getPurchases(ILjava/lang/String;Ljava/lang/String;Ljava/lang/String;)Landroid/os/Bundle;

    move-result-object v4

    .line 859
    .local v4, "ownedItems":Landroid/os/Bundle;
    move-object/from16 v0, p0

    invoke-virtual {v0, v4}, Lcom/outlinegames/unibill/IabHelper;->getResponseCodeFromBundle(Landroid/os/Bundle;)I

    move-result v9

    .line 860
    .local v9, "response":I
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Owned items response: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {v9}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v15

    invoke-virtual {v14, v15}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 861
    if-eqz v9, :cond_2

    .line 862
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "getPurchases() failed: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {v9}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v15

    invoke-virtual {v14, v15}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    goto/16 :goto_0

    .line 865
    :cond_2
    const-string v14, "INAPP_PURCHASE_ITEM_LIST"

    invoke-virtual {v4, v14}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v14

    if-eqz v14, :cond_3

    .line 866
    const-string v14, "INAPP_PURCHASE_DATA_LIST"

    invoke-virtual {v4, v14}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v14

    if-eqz v14, :cond_3

    .line 867
    const-string v14, "INAPP_DATA_SIGNATURE_LIST"

    invoke-virtual {v4, v14}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v14

    if-nez v14, :cond_4

    .line 868
    :cond_3
    const-string v14, "Bundle returned from getPurchases() doesn\'t contain required fields."

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 869
    const/16 v9, -0x3ea

    goto/16 :goto_0

    .line 873
    :cond_4
    const-string v14, "INAPP_PURCHASE_ITEM_LIST"

    .line 872
    invoke-virtual {v4, v14}, Landroid/os/Bundle;->getStringArrayList(Ljava/lang/String;)Ljava/util/ArrayList;

    move-result-object v5

    .line 875
    .local v5, "ownedSkus":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    const-string v14, "INAPP_PURCHASE_DATA_LIST"

    .line 874
    invoke-virtual {v4, v14}, Landroid/os/Bundle;->getStringArrayList(Ljava/lang/String;)Ljava/util/ArrayList;

    move-result-object v8

    .line 877
    .local v8, "purchaseDataList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    const-string v14, "INAPP_DATA_SIGNATURE_LIST"

    .line 876
    invoke-virtual {v4, v14}, Landroid/os/Bundle;->getStringArrayList(Ljava/lang/String;)Ljava/util/ArrayList;

    move-result-object v11

    .line 879
    .local v11, "signatureList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    const/4 v3, 0x0

    .local v3, "i":I
    :goto_1
    invoke-virtual {v8}, Ljava/util/ArrayList;->size()I

    move-result v14

    if-lt v3, v14, :cond_5

    .line 903
    const-string v14, "INAPP_CONTINUATION_TOKEN"

    invoke-virtual {v4, v14}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    .line 904
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Continuation token: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 905
    invoke-static {v2}, Landroid/text/TextUtils;->isEmpty(Ljava/lang/CharSequence;)Z

    move-result v14

    if-eqz v14, :cond_1

    .line 906
    if-eqz v13, :cond_8

    const/16 v14, -0x3eb

    :goto_2
    move v9, v14

    goto/16 :goto_0

    .line 880
    :cond_5
    invoke-virtual {v8, v3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v7

    check-cast v7, Ljava/lang/String;

    .line 881
    .local v7, "purchaseData":Ljava/lang/String;
    invoke-virtual {v11, v3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v10

    check-cast v10, Ljava/lang/String;

    .line 882
    .local v10, "signature":Ljava/lang/String;
    invoke-virtual {v5, v3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v12

    check-cast v12, Ljava/lang/String;

    .line 883
    .local v12, "sku":Ljava/lang/String;
    move-object/from16 v0, p0

    iget-object v14, v0, Lcom/outlinegames/unibill/IabHelper;->mSignatureBase64:Ljava/lang/String;

    invoke-static {v14, v7, v10}, Lcom/outlinegames/unibill/Security;->verifyPurchase(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Z

    move-result v14

    if-eqz v14, :cond_7

    .line 884
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Sku is owned: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v12}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 885
    new-instance v6, Lcom/outlinegames/unibill/Purchase;

    move-object/from16 v0, p2

    invoke-direct {v6, v0, v7, v10}, Lcom/outlinegames/unibill/Purchase;-><init>(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    .line 887
    .local v6, "purchase":Lcom/outlinegames/unibill/Purchase;
    invoke-virtual {v6}, Lcom/outlinegames/unibill/Purchase;->getToken()Ljava/lang/String;

    move-result-object v14

    invoke-static {v14}, Landroid/text/TextUtils;->isEmpty(Ljava/lang/CharSequence;)Z

    move-result v14

    if-eqz v14, :cond_6

    .line 888
    const-string v14, "BUG: empty/null token!"

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logWarn(Ljava/lang/String;)V

    .line 889
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "Purchase data: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 893
    :cond_6
    move-object/from16 v0, p1

    invoke-virtual {v0, v6}, Lcom/outlinegames/unibill/Inventory;->addPurchase(Lcom/outlinegames/unibill/Purchase;)V

    .line 879
    .end local v6    # "purchase":Lcom/outlinegames/unibill/Purchase;
    :goto_3
    add-int/lit8 v3, v3, 0x1

    goto/16 :goto_1

    .line 896
    :cond_7
    const-string v14, "Purchase signature verification **FAILED**. Not adding item."

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logWarn(Ljava/lang/String;)V

    .line 897
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "   Purchase data: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 898
    new-instance v14, Ljava/lang/StringBuilder;

    const-string v15, "   Signature: "

    invoke-direct {v14, v15}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v14, v10}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v14

    invoke-virtual {v14}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v14

    move-object/from16 v0, p0

    invoke-virtual {v0, v14}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 899
    const/4 v13, 0x1

    goto :goto_3

    .line 906
    .end local v7    # "purchaseData":Ljava/lang/String;
    .end local v10    # "signature":Ljava/lang/String;
    .end local v12    # "sku":Ljava/lang/String;
    :cond_8
    const/4 v14, 0x0

    goto/16 :goto_2
.end method

.method querySkuDetails(Ljava/lang/String;Lcom/outlinegames/unibill/Inventory;Ljava/util/List;Lcom/android/vending/billing/IInAppBillingService;)I
    .locals 14
    .param p1, "itemType"    # Ljava/lang/String;
    .param p2, "inv"    # Lcom/outlinegames/unibill/Inventory;
    .param p4, "service"    # Lcom/android/vending/billing/IInAppBillingService;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/String;",
            "Lcom/outlinegames/unibill/Inventory;",
            "Ljava/util/List",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Lcom/android/vending/billing/IInAppBillingService;",
            ")I"
        }
    .end annotation

    .annotation system Ldalvik/annotation/Throws;
        value = {
            Landroid/os/RemoteException;,
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 911
    .local p3, "moreSkus":Ljava/util/List;, "Ljava/util/List<Ljava/lang/String;>;"
    const-string v11, "Querying SKU details."

    invoke-virtual {p0, v11}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 912
    new-instance v8, Ljava/util/ArrayList;

    invoke-direct {v8}, Ljava/util/ArrayList;-><init>()V

    .line 913
    .local v8, "skuList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    move-object/from16 v0, p2

    invoke-virtual {v0, p1}, Lcom/outlinegames/unibill/Inventory;->getAllOwnedSkus(Ljava/lang/String;)Ljava/util/List;

    move-result-object v11

    invoke-virtual {v8, v11}, Ljava/util/ArrayList;->addAll(Ljava/util/Collection;)Z

    .line 914
    if-eqz p3, :cond_0

    move-object/from16 v0, p3

    invoke-virtual {v8, v0}, Ljava/util/ArrayList;->addAll(Ljava/util/Collection;)Z

    .line 916
    :cond_0
    invoke-virtual {v8}, Ljava/util/ArrayList;->size()I

    move-result v11

    if-nez v11, :cond_5

    .line 917
    const-string v11, "queryPrices: nothing to do because there are no SKUs."

    invoke-virtual {p0, v11}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 918
    const/4 v5, 0x0

    .line 958
    :goto_0
    return v5

    .line 922
    :cond_1
    const/16 v11, 0x14

    invoke-virtual {v8}, Ljava/util/ArrayList;->size()I

    move-result v12

    invoke-static {v11, v12}, Ljava/lang/Math;->min(II)I

    move-result v3

    .line 924
    .local v3, "endIndex":I
    new-instance v1, Ljava/util/ArrayList;

    invoke-direct {v1}, Ljava/util/ArrayList;-><init>()V

    .line 925
    .local v1, "chunk":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    const/4 v9, 0x0

    .local v9, "t":I
    :goto_1
    if-lt v9, v3, :cond_2

    .line 929
    new-instance v4, Landroid/os/Bundle;

    invoke-direct {v4}, Landroid/os/Bundle;-><init>()V

    .line 930
    .local v4, "querySkus":Landroid/os/Bundle;
    const-string v11, "ITEM_ID_LIST"

    invoke-virtual {v4, v11, v1}, Landroid/os/Bundle;->putStringArrayList(Ljava/lang/String;Ljava/util/ArrayList;)V

    .line 931
    const/4 v11, 0x3

    iget-object v12, p0, Lcom/outlinegames/unibill/IabHelper;->mContext:Landroid/content/Context;

    invoke-virtual {v12}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v12

    move-object/from16 v0, p4

    invoke-interface {v0, v11, v12, p1, v4}, Lcom/android/vending/billing/IInAppBillingService;->getSkuDetails(ILjava/lang/String;Ljava/lang/String;Landroid/os/Bundle;)Landroid/os/Bundle;

    move-result-object v7

    .line 934
    .local v7, "skuDetails":Landroid/os/Bundle;
    invoke-virtual {v8, v1}, Ljava/util/ArrayList;->removeAll(Ljava/util/Collection;)Z

    .line 936
    const-string v11, "DETAILS_LIST"

    invoke-virtual {v7, v11}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v11

    if-nez v11, :cond_4

    .line 937
    invoke-virtual {p0, v7}, Lcom/outlinegames/unibill/IabHelper;->getResponseCodeFromBundle(Landroid/os/Bundle;)I

    move-result v5

    .line 938
    .local v5, "response":I
    if-eqz v5, :cond_3

    .line 939
    new-instance v11, Ljava/lang/StringBuilder;

    const-string v12, "getSkuDetails() failed: "

    invoke-direct {v11, v12}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {v5}, Lcom/outlinegames/unibill/IabHelper;->getResponseDesc(I)Ljava/lang/String;

    move-result-object v12

    invoke-virtual {v11, v12}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v11

    invoke-virtual {v11}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v11

    invoke-virtual {p0, v11}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    goto :goto_0

    .line 926
    .end local v4    # "querySkus":Landroid/os/Bundle;
    .end local v5    # "response":I
    .end local v7    # "skuDetails":Landroid/os/Bundle;
    :cond_2
    invoke-virtual {v8, v9}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v11

    check-cast v11, Ljava/lang/String;

    invoke-virtual {v1, v11}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    .line 925
    add-int/lit8 v9, v9, 0x1

    goto :goto_1

    .line 943
    .restart local v4    # "querySkus":Landroid/os/Bundle;
    .restart local v5    # "response":I
    .restart local v7    # "skuDetails":Landroid/os/Bundle;
    :cond_3
    const-string v11, "getSkuDetails() returned a bundle with neither an error nor a detail list."

    invoke-virtual {p0, v11}, Lcom/outlinegames/unibill/IabHelper;->logError(Ljava/lang/String;)V

    .line 944
    const/16 v5, -0x3ea

    goto :goto_0

    .line 949
    .end local v5    # "response":I
    :cond_4
    const-string v11, "DETAILS_LIST"

    .line 948
    invoke-virtual {v7, v11}, Landroid/os/Bundle;->getStringArrayList(Ljava/lang/String;)Ljava/util/ArrayList;

    move-result-object v6

    .line 951
    .local v6, "responseList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    invoke-virtual {v6}, Ljava/util/ArrayList;->iterator()Ljava/util/Iterator;

    move-result-object v11

    :goto_2
    invoke-interface {v11}, Ljava/util/Iterator;->hasNext()Z

    move-result v12

    if-nez v12, :cond_6

    .line 921
    .end local v1    # "chunk":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    .end local v3    # "endIndex":I
    .end local v4    # "querySkus":Landroid/os/Bundle;
    .end local v6    # "responseList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    .end local v7    # "skuDetails":Landroid/os/Bundle;
    .end local v9    # "t":I
    :cond_5
    invoke-virtual {v8}, Ljava/util/ArrayList;->size()I

    move-result v11

    if-gtz v11, :cond_1

    .line 958
    const/4 v5, 0x0

    goto :goto_0

    .line 951
    .restart local v1    # "chunk":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    .restart local v3    # "endIndex":I
    .restart local v4    # "querySkus":Landroid/os/Bundle;
    .restart local v6    # "responseList":Ljava/util/ArrayList;, "Ljava/util/ArrayList<Ljava/lang/String;>;"
    .restart local v7    # "skuDetails":Landroid/os/Bundle;
    .restart local v9    # "t":I
    :cond_6
    invoke-interface {v11}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v10

    check-cast v10, Ljava/lang/String;

    .line 952
    .local v10, "thisResponse":Ljava/lang/String;
    new-instance v2, Lcom/outlinegames/unibill/SkuDetails;

    invoke-direct {v2, p1, v10}, Lcom/outlinegames/unibill/SkuDetails;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    .line 953
    .local v2, "d":Lcom/outlinegames/unibill/SkuDetails;
    new-instance v12, Ljava/lang/StringBuilder;

    const-string v13, "Got sku details: "

    invoke-direct {v12, v13}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v12, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v12

    invoke-virtual {v12}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v12

    invoke-virtual {p0, v12}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 954
    move-object/from16 v0, p2

    invoke-virtual {v0, v2}, Lcom/outlinegames/unibill/Inventory;->addSkuDetails(Lcom/outlinegames/unibill/SkuDetails;)V

    goto :goto_2
.end method

.method public startSetup(Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;)V
    .locals 2
    .param p1, "listener"    # Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;

    .prologue
    .line 205
    iget-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSetupDone:Z

    if-eqz v0, :cond_0

    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "IAB helper is already set up."

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 208
    :cond_0
    const-string v0, "Starting in-app billing setup."

    invoke-virtual {p0, v0}, Lcom/outlinegames/unibill/IabHelper;->logDebug(Ljava/lang/String;)V

    .line 210
    sget-object v0, Lcom/outlinegames/unibill/IabHelper;->serviceManager:Lcom/outlinegames/unibill/BillingServiceManager;

    new-instance v1, Lcom/outlinegames/unibill/IabHelper$1;

    invoke-direct {v1, p0, p1}, Lcom/outlinegames/unibill/IabHelper$1;-><init>(Lcom/outlinegames/unibill/IabHelper;Lcom/outlinegames/unibill/IabHelper$OnIabSetupFinishedListener;)V

    invoke-virtual {v0, v1}, Lcom/outlinegames/unibill/BillingServiceManager;->initialise(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V

    .line 227
    return-void
.end method

.method public subscriptionsSupported()Z
    .locals 1

    .prologue
    .line 300
    iget-boolean v0, p0, Lcom/outlinegames/unibill/IabHelper;->mSubscriptionsSupported:Z

    return v0
.end method
