.class Lcom/outlinegames/unibill/UniBill$6;
.super Ljava/lang/Object;
.source "UniBill.java"

# interfaces
.implements Lcom/outlinegames/unibill/IabHelper$OnConsumeFinishedListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/outlinegames/unibill/UniBill;->ConsumeProductAndTellUnity(Lcom/outlinegames/unibill/Purchase;J)V
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
    iput-object p1, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    .line 299
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public onConsumeFinished(Lcom/outlinegames/unibill/Purchase;Lcom/outlinegames/unibill/IabResult;)V
    .locals 4
    .param p1, "purchase"    # Lcom/outlinegames/unibill/Purchase;
    .param p2, "result"    # Lcom/outlinegames/unibill/IabResult;
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    .prologue
    .line 303
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    const-string v1, "onConsumeFinished:%s"

    invoke-virtual {p2}, Lcom/outlinegames/unibill/IabResult;->isSuccess()Z

    move-result v2

    invoke-static {v2}, Ljava/lang/Boolean;->toString(Z)Ljava/lang/String;

    move-result-object v2

    invoke-static {v0, v1, v2}, Lcom/outlinegames/unibill/UniBill;->access$0(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;Ljava/lang/String;)V

    .line 304
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    iget-object v1, p2, Lcom/outlinegames/unibill/IabResult;->mMessage:Ljava/lang/String;

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/UniBill;->access$1(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;)V

    .line 305
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    invoke-virtual {p2}, Lcom/outlinegames/unibill/IabResult;->getResponse()I

    move-result v1

    invoke-static {v1}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/UniBill;->access$1(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;)V

    .line 306
    invoke-virtual {p2}, Lcom/outlinegames/unibill/IabResult;->isSuccess()Z

    move-result v0

    if-eqz v0, :cond_1

    .line 307
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    invoke-static {v0, p1}, Lcom/outlinegames/unibill/UniBill;->access$5(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Purchase;)V

    .line 314
    :cond_0
    :goto_0
    return-void

    .line 309
    :cond_1
    invoke-virtual {p2}, Lcom/outlinegames/unibill/IabResult;->getResponse()I

    move-result v0

    const/4 v1, 0x2

    if-ne v0, v1, :cond_0

    .line 310
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    const-string v1, "Consumption failed. Retrying.."

    invoke-static {v0, v1}, Lcom/outlinegames/unibill/UniBill;->access$1(Lcom/outlinegames/unibill/UniBill;Ljava/lang/String;)V

    .line 311
    iget-object v0, p0, Lcom/outlinegames/unibill/UniBill$6;->this$0:Lcom/outlinegames/unibill/UniBill;

    const-wide/16 v2, 0x3e8

    invoke-static {v0, p1, v2, v3}, Lcom/outlinegames/unibill/UniBill;->access$4(Lcom/outlinegames/unibill/UniBill;Lcom/outlinegames/unibill/Purchase;J)V

    goto :goto_0
.end method
