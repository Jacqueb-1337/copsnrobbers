.class final Lcom/kamcord/android/core/KC_q;
.super Lcom/kamcord/android/core/KC_s;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_y;


# direct methods
.method constructor <init>()V
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_s;-><init>()V

    new-instance v0, Lcom/kamcord/android/core/KC_y;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_y;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_q;->b:Lcom/kamcord/android/core/KC_y;

    return-void
.end method


# virtual methods
.method final a()V
    .locals 0

    return-void
.end method

.method protected final a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_q;->b:Lcom/kamcord/android/core/KC_y;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_y;->a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V

    return-void
.end method

.method final b()V
    .locals 0

    return-void
.end method

.method protected final c()Lcom/kamcord/android/core/KC_y;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_q;->b:Lcom/kamcord/android/core/KC_y;

    return-object v0
.end method

.method final d()Lcom/kamcord/android/core/KC_y;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_q;->b:Lcom/kamcord/android/core/KC_y;

    return-object v0
.end method

.method protected final e()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_q;->b:Lcom/kamcord/android/core/KC_y;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_y;->a()V

    return-void
.end method
