.class final Lcom/kamcord/android/KC_N$2;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->localVideoFailed(Lcom/kamcord/android/core/KC_H;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$2;->a:Lcom/kamcord/android/KC_N;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_N$2;->a:Lcom/kamcord/android/KC_N;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_N;->b()V

    return-void
.end method
