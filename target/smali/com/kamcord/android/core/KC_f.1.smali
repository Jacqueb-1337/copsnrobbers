.class public final Lcom/kamcord/android/core/KC_f;
.super Ljava/lang/Object;


# instance fields
.field public a:I

.field public b:I


# direct methods
.method public constructor <init>(II)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput p1, p0, Lcom/kamcord/android/core/KC_f;->a:I

    iput p2, p0, Lcom/kamcord/android/core/KC_f;->b:I

    return-void
.end method


# virtual methods
.method public final equals(Ljava/lang/Object;)Z
    .locals 4

    const/4 v1, 0x1

    const/4 v0, 0x0

    if-nez p1, :cond_1

    :cond_0
    :goto_0
    return v0

    :cond_1
    if-ne p1, p0, :cond_2

    move v0, v1

    goto :goto_0

    :cond_2
    instance-of v2, p1, Lcom/kamcord/android/core/KC_f;

    if-eqz v2, :cond_0

    check-cast p1, Lcom/kamcord/android/core/KC_f;

    iget v2, p0, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->a:I

    if-ne v2, v3, :cond_0

    iget v2, p0, Lcom/kamcord/android/core/KC_f;->b:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    if-ne v2, v3, :cond_0

    move v0, v1

    goto :goto_0
.end method

.method public final hashCode()I
    .locals 2

    iget v0, p0, Lcom/kamcord/android/core/KC_f;->b:I

    add-int/lit8 v0, v0, 0x11

    mul-int/lit8 v0, v0, 0x1f

    iget v1, p0, Lcom/kamcord/android/core/KC_f;->a:I

    add-int/2addr v0, v1

    return v0
.end method
