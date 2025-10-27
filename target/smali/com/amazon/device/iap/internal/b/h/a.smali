.class public Lcom/amazon/device/iap/internal/b/h/a;
.super Lcom/amazon/device/iap/internal/b/i;
.source "SubmitMetricCommand.java"


# static fields
.field private static final a:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 16
    const-class v0, Lcom/amazon/device/iap/internal/b/h/a;

    invoke-virtual {v0}, Ljava/lang/Class;->getSimpleName()Ljava/lang/String;

    move-result-object v0

    sput-object v0, Lcom/amazon/device/iap/internal/b/h/a;->a:Ljava/lang/String;

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/b/e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 2

    .prologue
    .line 28
    const-string v0, "submit_metric"

    const-string v1, "1.0"

    invoke-direct {p0, p1, v0, v1}, Lcom/amazon/device/iap/internal/b/i;-><init>(Lcom/amazon/device/iap/internal/b/e;Ljava/lang/String;Ljava/lang/String;)V

    .line 30
    const-string v0, "metricName"

    invoke-virtual {p0, v0, p2}, Lcom/amazon/device/iap/internal/b/h/a;->a(Ljava/lang/String;Ljava/lang/Object;)V

    .line 31
    const-string v0, "metricAttributes"

    invoke-virtual {p0, v0, p3}, Lcom/amazon/device/iap/internal/b/h/a;->a(Ljava/lang/String;Ljava/lang/Object;)V

    .line 33
    const/4 v0, 0x0

    invoke-virtual {p0, v0}, Lcom/amazon/device/iap/internal/b/h/a;->b(Z)V

    .line 34
    return-void
.end method


# virtual methods
.method protected a(Lcom/amazon/venezia/command/SuccessResult;)Z
    .locals 1
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Landroid/os/RemoteException;,
            Lcom/amazon/android/framework/exception/KiwiException;
        }
    .end annotation

    .prologue
    .line 38
    const/4 v0, 0x1

    return v0
.end method
