.class Lcom/outlinegames/unibill/BillingServiceManager$1;
.super Ljava/lang/Object;
.source "BillingServiceManager.java"

# interfaces
.implements Landroid/content/ServiceConnection;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/outlinegames/unibill/BillingServiceManager;->initialise(Lcom/outlinegames/unibill/BillingServiceManager$BillingServiceProcessor;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic this$0:Lcom/outlinegames/unibill/BillingServiceManager;


# direct methods
.method constructor <init>(Lcom/outlinegames/unibill/BillingServiceManager;)V
    .locals 0

    .prologue
    .line 1
    iput-object p1, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    .line 44
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/BillingServiceManager$1;)Lcom/outlinegames/unibill/BillingServiceManager;
    .locals 1

    .prologue
    .line 44
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    return-object v0
.end method


# virtual methods
.method public onServiceConnected(Landroid/content/ComponentName;Landroid/os/IBinder;)V
    .locals 2
    .param p1, "name"    # Landroid/content/ComponentName;
    .param p2, "service"    # Landroid/os/IBinder;

    .prologue
    .line 59
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    const-string v1, "Billing service connected."

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/BillingServiceManager;->access$0(Lcom/outlinegames/unibill/BillingServiceManager;Ljava/lang/String;)V

    .line 60
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    invoke-static {v0}, Lcom/outlinegames/unibill/BillingServiceManager;->access$1(Lcom/outlinegames/unibill/BillingServiceManager;)Ljava/util/concurrent/ExecutorService;

    move-result-object v0

    new-instance v1, Lcom/outlinegames/unibill/BillingServiceManager$1$2;

    invoke-direct {v1, p0, p2}, Lcom/outlinegames/unibill/BillingServiceManager$1$2;-><init>(Lcom/outlinegames/unibill/BillingServiceManager$1;Landroid/os/IBinder;)V

    invoke-interface {v0, v1}, Ljava/util/concurrent/ExecutorService;->execute(Ljava/lang/Runnable;)V

    .line 68
    return-void
.end method

.method public onServiceDisconnected(Landroid/content/ComponentName;)V
    .locals 2
    .param p1, "name"    # Landroid/content/ComponentName;

    .prologue
    .line 47
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    const-string v1, "Billing service disconnected."

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/BillingServiceManager;->access$0(Lcom/outlinegames/unibill/BillingServiceManager;Ljava/lang/String;)V

    .line 48
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1;->this$0:Lcom/outlinegames/unibill/BillingServiceManager;

    invoke-static {v0}, Lcom/outlinegames/unibill/BillingServiceManager;->access$1(Lcom/outlinegames/unibill/BillingServiceManager;)Ljava/util/concurrent/ExecutorService;

    move-result-object v0

    new-instance v1, Lcom/outlinegames/unibill/BillingServiceManager$1$1;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/BillingServiceManager$1$1;-><init>(Lcom/outlinegames/unibill/BillingServiceManager$1;)V

    invoke-interface {v0, v1}, Ljava/util/concurrent/ExecutorService;->execute(Ljava/lang/Runnable;)V

    .line 55
    return-void
.end method
