.class Lcom/outlinegames/unibill/BillingServiceManager$1$1;
.super Ljava/lang/Object;
.source "BillingServiceManager.java"

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/outlinegames/unibill/BillingServiceManager$1;->onServiceDisconnected(Landroid/content/ComponentName;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic this$1:Lcom/outlinegames/unibill/BillingServiceManager$1;


# direct methods
.method constructor <init>(Lcom/outlinegames/unibill/BillingServiceManager$1;)V
    .locals 0

    .prologue
    .line 1
    iput-object p1, p0, Lcom/outlinegames/unibill/BillingServiceManager$1$1;->this$1:Lcom/outlinegames/unibill/BillingServiceManager$1;

    .line 48
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public run()V
    .locals 2

    .prologue
    .line 51
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1$1;->this$1:Lcom/outlinegames/unibill/BillingServiceManager$1;

    invoke-static {v0}, Lcom/outlinegames/unibill/BillingServiceManager$1;->access$0(Lcom/outlinegames/unibill/BillingServiceManager$1;)Lcom/outlinegames/unibill/BillingServiceManager;

    move-result-object v0

    invoke-static {v0}, Lcom/outlinegames/unibill/BillingServiceManager;->access$2(Lcom/outlinegames/unibill/BillingServiceManager;)Landroid/content/Context;

    move-result-object v0

    iget-object v1, p0, Lcom/outlinegames/unibill/BillingServiceManager$1$1;->this$1:Lcom/outlinegames/unibill/BillingServiceManager$1;

    invoke-static {v1}, Lcom/outlinegames/unibill/BillingServiceManager$1;->access$0(Lcom/outlinegames/unibill/BillingServiceManager$1;)Lcom/outlinegames/unibill/BillingServiceManager;

    move-result-object v1

    invoke-static {v1}, Lcom/outlinegames/unibill/BillingServiceManager;->access$3(Lcom/outlinegames/unibill/BillingServiceManager;)Landroid/content/ServiceConnection;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/content/Context;->unbindService(Landroid/content/ServiceConnection;)V

    .line 52
    iget-object v0, p0, Lcom/outlinegames/unibill/BillingServiceManager$1$1;->this$1:Lcom/outlinegames/unibill/BillingServiceManager$1;

    invoke-static {v0}, Lcom/outlinegames/unibill/BillingServiceManager$1;->access$0(Lcom/outlinegames/unibill/BillingServiceManager$1;)Lcom/outlinegames/unibill/BillingServiceManager;

    move-result-object v0

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/BillingServiceManager;->access$4(Lcom/outlinegames/unibill/BillingServiceManager;Lcom/android/vending/billing/IInAppBillingService;)V

    .line 53
    return-void
.end method
