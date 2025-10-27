.class abstract Lcom/kamcord/android/core/KC_I;
.super Ljava/lang/Object;


# instance fields
.field protected a:Lcom/kamcord/android/core/KC_w;

.field private b:J

.field private c:J

.field private d:J

.field private e:J

.field private f:Z


# direct methods
.method constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method protected final a(J)J
    .locals 7

    const-wide/32 v0, 0x989680

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->c:J

    sub-long v2, p1, v2

    cmp-long v4, v2, v0

    if-gez v4, :cond_3

    :goto_0
    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->b:J

    add-long/2addr v0, v2

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_I;->f:Z

    if-eqz v2, :cond_1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_I;->f:Z

    iget-wide v0, p0, Lcom/kamcord/android/core/KC_I;->e:J

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->b:J

    cmp-long v0, v0, v2

    if-gez v0, :cond_0

    iget-wide v0, p0, Lcom/kamcord/android/core/KC_I;->b:J

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_I;->e:J

    :cond_0
    iget-wide v0, p0, Lcom/kamcord/android/core/KC_I;->e:J

    :cond_1
    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->d:J

    const-wide/16 v4, 0x0

    cmp-long v2, v2, v4

    if-nez v2, :cond_2

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_I;->d:J

    :cond_2
    iput-wide p1, p0, Lcom/kamcord/android/core/KC_I;->c:J

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_I;->b:J

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->d:J

    sub-long/2addr v0, v2

    return-wide v0

    :cond_3
    move-wide v0, v2

    goto :goto_0
.end method

.method final a(D)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_I;->a:Lcom/kamcord/android/core/KC_w;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_I;->a:Lcom/kamcord/android/core/KC_w;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/android/core/KC_w;->b(D)V

    :cond_0
    return-void
.end method

.method abstract a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/core/KC_f;)Z
.end method

.method abstract a(Lcom/kamcord/android/core/KC_y;J)Z
.end method

.method final b(J)V
    .locals 1

    iput-wide p1, p0, Lcom/kamcord/android/core/KC_I;->e:J

    return-void
.end method

.method abstract c()[I
.end method

.method abstract d()V
.end method

.method abstract e()V
.end method

.method abstract f()V
.end method

.method abstract g()Z
.end method

.method protected final h()V
    .locals 4

    const-wide/16 v2, 0x0

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_I;->f:Z

    iput-wide v2, p0, Lcom/kamcord/android/core/KC_I;->b:J

    iput-wide v2, p0, Lcom/kamcord/android/core/KC_I;->d:J

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_I;->c:J

    return-void
.end method

.method final i()D
    .locals 4

    const-wide v0, 0x3e112e0be826d695L    # 1.0E-9

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_I;->b:J

    long-to-double v2, v2

    mul-double/2addr v0, v2

    return-wide v0
.end method

.method final j()V
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_I;->f:Z

    return-void
.end method

.method final k()V
    .locals 2

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_I;->f:Z

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_I;->c:J

    return-void
.end method
