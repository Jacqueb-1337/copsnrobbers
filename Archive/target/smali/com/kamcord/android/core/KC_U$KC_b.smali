.class final Lcom/kamcord/android/core/KC_U$KC_b;
.super Lcom/kamcord/android/KC_ad;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/core/KC_U;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_b"
.end annotation


# instance fields
.field private synthetic b:Lcom/kamcord/android/core/KC_U;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_U;)V
    .locals 1

    iput-object p1, p0, Lcom/kamcord/android/core/KC_U$KC_b;->b:Lcom/kamcord/android/core/KC_U;

    const-string v0, "kamcord-voice"

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_ad;-><init>(Ljava/lang/String;)V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/core/KC_U$KC_b;I)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_b;->a:Lcom/kamcord/android/KC_ac;

    invoke-virtual {v0, p1}, Lcom/kamcord/android/KC_ac;->sendEmptyMessage(I)Z

    return-void
.end method


# virtual methods
.method protected final a()Lcom/kamcord/android/KC_ac;
    .locals 3

    new-instance v0, Lcom/kamcord/android/core/KC_U$KC_a;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_b;->b:Lcom/kamcord/android/core/KC_U;

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_U$KC_b;->getLooper()Landroid/os/Looper;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/core/KC_U$KC_a;-><init>(Lcom/kamcord/android/core/KC_U;Landroid/os/Looper;)V

    return-object v0
.end method
