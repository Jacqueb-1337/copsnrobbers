.class public Lcom/outlinegames/unibill/BillingServiceManager;
.super Ljava/lang/Object;
.source "BillingServiceManager.java"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;
    }
.end annotation


# instance fields
.field private final callbacks:Ljava/util/concurrent/ConcurrentLinkedQueue;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/concurrent/ConcurrentLinkedQueue",
            "<",
            "Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;",
            ">;"
        }
    .end annotation
.end field

.field private context:Landroid/content/Context;

.field private executor:Ljava/util/concurrent/ExecutorService;

.field private volatile mService:Lcom/android/vending/billing/IInAppBillingService;

.field private volatile mServiceConn:Landroid/content/ServiceConnection;


# direct methods
.method public constructor <init>(Landroid/content/Context;)V
    .locals 1
    .param p1, "context"    # Landroid/content/Context;

    .prologue
    .line 36
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 30
    invoke-static {}, Ljava/util/concurrent/Executors;->newSingleThreadExecutor()Ljava/util/concurrent/ExecutorService;

    move-result-object v0

    iput-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->executor:Ljava/util/concurrent/ExecutorService;

    .line 40
    new-instance v0, Ljava/util/concurrent/ConcurrentLinkedQueue;

    invoke-direct {v0}, Ljava/util/concurrent/ConcurrentLinkedQueue;-><init>()V

    iput-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->callbacks:Ljava/util/concurrent/ConcurrentLinkedQueue;

    .line 37
    iput-object p1, p0, Lcom/outlinegames/unibill/BillingServiceManager;->context:Landroid/content/Context;

    .line 38
    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/BillingServiceManager;Ljava/lang/String;)V
    .locals 0

    .prologue
    .line 122
    invoke-direct {p0, p1}, Lcom/outlinegames/unibill/BillingServiceManager;->logDebug(Ljava/lang/String;)V

    return-void
.end method

.method static synthetic access$1(Lcom/outlinegames/unibill/BillingServiceManager;)Ljava/util/concurrent/ExecutorService;
    .locals 1

    .prologue
    .line 30
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->executor:Ljava/util/concurrent/ExecutorService;

    return-object v0
.end method

.method static synthetic access$2(Lcom/outlinegames/unibill/BillingServiceManager;)Landroid/content/Context;
    .locals 1

    .prologue
    .line 29
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->context:Landroid/content/Context;

    return-object v0
.end method

.method static synthetic access$3(Lcom/outlinegames/unibill/BillingServiceManager;)Landroid/content/ServiceConnection;
    .locals 1

    .prologue
    .line 28
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mServiceConn:Landroid/content/ServiceConnection;

    return-object v0
.end method

.method static synthetic access$4(Lcom/outlinegames/unibill/BillingServiceManager;Lcom/android/vending/billing/IInAppBillingService;)V
    .locals 0

    .prologue
    .line 27
    iput-object p1, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mService:Lcom/android/vending/billing/IInAppBillingService;

    return-void
.end method

.method static synthetic access$5(Lcom/outlinegames/unibill/BillingServiceManager;)V
    .locals 0

    .prologue
    .line 106
    invoke-direct {p0}, Lcom/outlinegames/unibill/BillingServiceManager;->pumpCallbacks()V

    return-void
.end method

.method static synthetic access$6(Lcom/outlinegames/unibill/BillingServiceManager;)Ljava/util/concurrent/ConcurrentLinkedQueue;
    .locals 1

    .prologue
    .line 40
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->callbacks:Ljava/util/concurrent/ConcurrentLinkedQueue;

    return-object v0
.end method

.method static synthetic access$7(Lcom/outlinegames/unibill/BillingServiceManager;)Lcom/android/vending/billing/IInAppBillingService;
    .locals 1

    .prologue
    .line 27
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mService:Lcom/android/vending/billing/IInAppBillingService;

    return-object v0
.end method

.method private logDebug(Ljava/lang/String;)V
    .locals 1
    .param p1, "msg"    # Ljava/lang/String;

    .prologue
    .line 123
    const-string v0, "Unibill"

    invoke-static {v0, p1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    .line 124
    return-void
.end method

.method private pumpCallbacks()V
    .locals 2

    .prologue
    .line 107
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->executor:Ljava/util/concurrent/ExecutorService;

    new-instance v1, Lcom/outlinegames/unibill/BillingServiceManager$2;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/BillingServiceManager$2;-><init>(Lcom/outlinegames/unibill/BillingServiceManager;)V

    invoke-interface {v0, v1}, Ljava/util/concurrent/ExecutorService;->execute(Ljava/lang/Runnable;)V

    .line 120
    return-void
.end method


# virtual methods
.method public dispose()V
    .locals 2

    .prologue
    .line 103
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->context:Landroid/content/Context;

    iget-object v1, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mServiceConn:Landroid/content/ServiceConnection;

    invoke-virtual {v0, v1}, Landroid/content/Context;->unbindService(Landroid/content/ServiceConnection;)V

    .line 104
    return-void
.end method

.method initialise(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V
    .locals 7
    .param p1, "callback"    # Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;

    .prologue
    const/4 v6, 0x1

    .line 43
    iget-object v4, p0, Lcom/outlinegames/unibill/BillingServiceManager;->callbacks:Ljava/util/concurrent/ConcurrentLinkedQueue;

    invoke-virtual {v4, p1}, Ljava/util/concurrent/ConcurrentLinkedQueue;->add(Ljava/lang/Object;)Z

    .line 44
    new-instance v4, Lcom/outlinegames/unibill/BillingServiceManager$1;

    invoke-direct {v4, p0}, Lcom/outlinegames/unibill/BillingServiceManager$1;-><init>(Lcom/outlinegames/unibill/BillingServiceManager;)V

    iput-object v4, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mServiceConn:Landroid/content/ServiceConnection;

    .line 72
    :try_start_0
    new-instance v2, Landroid/content/Intent;

    const-string v4, "com.android.vending.billing.InAppBillingService.BIND"

    invoke-direct {v2, v4}, Landroid/content/Intent;-><init>(Ljava/lang/String;)V

    .line 73
    .local v2, "serviceIntent":Landroid/content/Intent;
    const-string v4, "com.android.vending"

    invoke-virtual {v2, v4}, Landroid/content/Intent;->setPackage(Ljava/lang/String;)Landroid/content/Intent;

    .line 74
    iget-object v4, p0, Lcom/outlinegames/unibill/BillingServiceManager;->context:Landroid/content/Context;

    invoke-virtual {v4}, Landroid/content/Context;->getPackageManager()Landroid/content/pm/PackageManager;

    move-result-object v4

    const/4 v5, 0x0

    invoke-virtual {v4, v2, v5}, Landroid/content/pm/PackageManager;->queryIntentServices(Landroid/content/Intent;I)Ljava/util/List;

    move-result-object v1

    .line 75
    .local v1, "list":Ljava/util/List;, "Ljava/util/List<Landroid/content/pm/ResolveInfo;>;"
    if-eqz v1, :cond_0

    invoke-interface {v1}, Ljava/util/List;->size()I

    move-result v4

    if-ne v4, v6, :cond_0

    .line 77
    iget-object v4, p0, Lcom/outlinegames/unibill/BillingServiceManager;->context:Landroid/content/Context;

    iget-object v5, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mServiceConn:Landroid/content/ServiceConnection;

    const/4 v6, 0x1

    invoke-virtual {v4, v2, v5, v6}, Landroid/content/Context;->bindService(Landroid/content/Intent;Landroid/content/ServiceConnection;I)Z
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_1

    .line 91
    :goto_0
    return-void

    .line 82
    :cond_0
    const/4 v4, 0x0

    :try_start_1
    invoke-interface {p1, v4}, Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;->workWith(Lcom/android/vending/billing/IInAppBillingService;)V
    :try_end_1
    .catch Lcom/outlinegames/unibill/IabException; {:try_start_1 .. :try_end_1} :catch_0
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_1

    goto :goto_0

    .line 83
    :catch_0
    move-exception v0

    .line 84
    .local v0, "e":Lcom/outlinegames/unibill/IabException;
    :try_start_2
    invoke-virtual {v0}, Lcom/outlinegames/unibill/IabException;->printStackTrace()V
    :try_end_2
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_1

    goto :goto_0

    .line 87
    .end local v0    # "e":Lcom/outlinegames/unibill/IabException;
    .end local v1    # "list":Ljava/util/List;, "Ljava/util/List<Landroid/content/pm/ResolveInfo;>;"
    .end local v2    # "serviceIntent":Landroid/content/Intent;
    :catch_1
    move-exception v3

    .line 88
    .local v3, "t":Ljava/lang/Throwable;
    invoke-virtual {v3}, Ljava/lang/Throwable;->printStackTrace()V

    .line 89
    new-instance v4, Ljava/lang/RuntimeException;

    invoke-direct {v4, v3}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/Throwable;)V

    throw v4
.end method

.method public workWith(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V
    .locals 1
    .param p1, "processor"    # Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;

    .prologue
    .line 94
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->mService:Lcom/android/vending/billing/IInAppBillingService;

    if-eqz v0, :cond_0

    .line 95
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager;->callbacks:Ljava/util/concurrent/ConcurrentLinkedQueue;

    invoke-virtual {v0, p1}, Ljava/util/concurrent/ConcurrentLinkedQueue;->add(Ljava/lang/Object;)Z

    .line 96
    invoke-direct {p0}, Lcom/outlinegames/unibill/BillingServiceManager;->pumpCallbacks()V

    .line 100
    :goto_0
    return-void

    .line 98
    :cond_0
    invoke-virtual {p0, p1}, Lcom/outlinegames/unibill/BillingServiceManager;->initialise(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V

    goto :goto_0
.end method
