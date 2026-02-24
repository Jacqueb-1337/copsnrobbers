.class final Lcom/kamcord/android/core/KC_r;
.super Lcom/kamcord/android/core/KC_s;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_b;


# direct methods
.method constructor <init>(I)V
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_s;-><init>()V

    new-instance v0, Lcom/kamcord/android/core/KC_b;

    invoke-direct {v0, p1}, Lcom/kamcord/android/core/KC_b;-><init>(I)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    return-void
.end method


# virtual methods
.method final a()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_b;->a()V

    return-void
.end method

.method protected final a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_b;->a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V

    return-void
.end method

.method final b()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_b;->a()V

    return-void
.end method

.method protected final c()Lcom/kamcord/android/core/KC_y;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_b;->b()Lcom/kamcord/android/core/KC_y;

    move-result-object v0

    return-object v0
.end method

.method final d()Lcom/kamcord/android/core/KC_y;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_b;->c()Lcom/kamcord/android/core/KC_y;

    move-result-object v0

    return-object v0
.end method

.method protected final e()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_r;->b:Lcom/kamcord/android/core/KC_b;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_b;->d()V

    return-void
.end method
