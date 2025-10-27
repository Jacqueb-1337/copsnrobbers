.class public final Lcom/amazon/device/iap/internal/b/f/b;
.super Lcom/amazon/device/iap/internal/b/f/a;
.source "ResponseReceivedCommandV1.java"


# static fields
.field private static final a:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 15
    const-class v0, Lcom/amazon/device/iap/internal/b/f/b;

    invoke-virtual {v0}, Ljava/lang/Class;->getSimpleName()Ljava/lang/String;

    move-result-object v0

    sput-object v0, Lcom/amazon/device/iap/internal/b/f/b;->a:Ljava/lang/String;

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/b/e;Ljava/lang/String;)V
    .locals 2

    .prologue
    .line 21
    const-string v0, "1.0"

    invoke-direct {p0, p1, v0}, Lcom/amazon/device/iap/internal/b/f/a;-><init>(Lcom/amazon/device/iap/internal/b/e;Ljava/lang/String;)V

    .line 22
    invoke-virtual {p0}, Lcom/amazon/device/iap/internal/b/f/b;->getCommandData()Ljava/util/Map;

    move-result-object v0

    const-string v1, "requestId"

    invoke-interface {v0, v1, p2}, Ljava/util/Map;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 23
    return-void
.end method
