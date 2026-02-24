.class final Lcom/kamcord/android/KC_N$13;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->localVideoReady(Lcom/kamcord/android/core/KC_H;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/core/KC_H;

.field private synthetic b:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$13;->b:Lcom/kamcord/android/KC_N;

    iput-object p2, p0, Lcom/kamcord/android/KC_N$13;->a:Lcom/kamcord/android/core/KC_H;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N$13;->b:Lcom/kamcord/android/KC_N;

    iget-object v1, p0, Lcom/kamcord/android/KC_N$13;->a:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/core/KC_H;)V

    return-void
.end method
