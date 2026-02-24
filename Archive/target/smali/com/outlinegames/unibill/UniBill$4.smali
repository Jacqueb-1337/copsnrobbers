.class Lcom/outlinegames/unibill/UniBill$4;
.super Ljava/lang/Object;
.source "UniBill.java"

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/outlinegames/unibill/UniBill;->restoreTransactions()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic this$0:Lcom/outlinegames/unibill/UniBill;


# direct methods
.method constructor <init>(Lcom/outlinegames/unibill/UniBill;)V
    .locals 0

    .prologue
    .line 1
    iput-object p1, p0, Lcom/outlinegames/unibill/UniBill$4;->this$0:Lcom/outlinegames/unibill/UniBill;

    .line 183
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method static synthetic access$0(Lcom/outlinegames/unibill/UniBill$4;)Lcom/outlinegames/unibill/UniBill;
    .locals 1

    .prologue
    .line 183
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$4;->this$0:Lcom/outlinegames/unibill/UniBill;

    return-object v0
.end method


# virtual methods
.method public run()V
    .locals 2

    .prologue
    .line 187
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$4;->this$0:Lcom/outlinegames/unibill/UniBill;

    iget-object v0, v0, Lcom/outlinegames/unibill/UniBill;->helper:Lcom/outlinegames/unibill/IabHelper;

    new-instance v1, Lcom/outlinegames/unibill/UniBill$4$1;

    invoke-direct {v1, p0}, Lcom/outlinegames/unibill/UniBill$4$1;-><init>(Lcom/outlinegames/unibill/UniBill$4;)V

    invoke-virtual {v0, v1}, Lcom/outlinegames/unibill/IabHelper;->queryInventoryAsync(Lcom/outlinegames/unibill/IabHelper$QueryInventoryFinishedListener;)V

    .line 209
    return-void
.end method
