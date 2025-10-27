.class final Lcom/kamcord/android/core/KC_b;
.super Ljava/lang/Object;


# instance fields
.field private a:I

.field private b:[Lcom/kamcord/android/core/KC_y;

.field private c:[Z

.field private volatile d:I

.field private volatile e:I


# direct methods
.method constructor <init>(I)V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput p1, p0, Lcom/kamcord/android/core/KC_b;->a:I

    iget v0, p0, Lcom/kamcord/android/core/KC_b;->a:I

    new-array v0, v0, [Lcom/kamcord/android/core/KC_y;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    iget v0, p0, Lcom/kamcord/android/core/KC_b;->a:I

    new-array v0, v0, [Z

    iput-object v0, p0, Lcom/kamcord/android/core/KC_b;->c:[Z

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_b;->a()V

    return-void
.end method


# virtual methods
.method final a()V
    .locals 4

    const/4 v1, 0x0

    iput v1, p0, Lcom/kamcord/android/core/KC_b;->d:I

    iput v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    move v0, v1

    :goto_0
    iget v2, p0, Lcom/kamcord/android/core/KC_b;->a:I

    if-ge v0, v2, :cond_1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_b;->c:[Z

    aput-boolean v1, v2, v0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v2, v2, v0

    if-eqz v2, :cond_0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v2, v2, v0

    const/4 v3, 0x1

    iput-boolean v3, v2, Lcom/kamcord/android/core/KC_y;->g:Z

    :cond_0
    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_1
    return-void
.end method

.method protected final a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V
    .locals 3

    const/4 v0, 0x0

    :goto_0
    iget v1, p0, Lcom/kamcord/android/core/KC_b;->a:I

    if-ge v0, v1, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v1, v1, v0

    if-nez v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    new-instance v2, Lcom/kamcord/android/core/KC_y;

    invoke-direct {v2}, Lcom/kamcord/android/core/KC_y;-><init>()V

    aput-object v2, v1, v0

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v1, v1, v0

    invoke-virtual {v1, p1, p2}, Lcom/kamcord/android/core/KC_y;->a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_1
    return-void
.end method

.method final b()Lcom/kamcord/android/core/KC_y;
    .locals 3

    const/4 v0, 0x0

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->d:I

    add-int/lit8 v1, v1, 0x1

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->a:I

    rem-int/2addr v1, v2

    iget-object v2, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v1, v2, v1

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_y;->g:Z

    if-eqz v1, :cond_0

    iget v0, p0, Lcom/kamcord/android/core/KC_b;->d:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/core/KC_b;->d:I

    iget v0, p0, Lcom/kamcord/android/core/KC_b;->d:I

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->a:I

    rem-int/2addr v0, v1

    iput v0, p0, Lcom/kamcord/android/core/KC_b;->d:I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_b;->c:[Z

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->d:I

    const/4 v2, 0x1

    aput-boolean v2, v0, v1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->d:I

    aget-object v0, v0, v1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->d:I

    aget-object v1, v1, v2

    const/4 v2, 0x0

    iput-boolean v2, v1, Lcom/kamcord/android/core/KC_y;->g:Z

    :cond_0
    return-object v0
.end method

.method final c()Lcom/kamcord/android/core/KC_y;
    .locals 4

    const/4 v0, 0x0

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->a:I

    const/4 v2, 0x1

    if-le v1, v2, :cond_0

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->d:I

    if-ne v1, v2, :cond_0

    :goto_0
    return-object v0

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->c:[Z

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->e:I

    aget-boolean v1, v1, v2

    if-eqz v1, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    aget-object v0, v0, v1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->c:[Z

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->e:I

    const/4 v3, 0x0

    aput-boolean v3, v1, v2

    :cond_1
    iget v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    add-int/lit8 v1, v1, 0x1

    iput v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    iget v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    iget v2, p0, Lcom/kamcord/android/core/KC_b;->a:I

    rem-int/2addr v1, v2

    iput v1, p0, Lcom/kamcord/android/core/KC_b;->e:I

    goto :goto_0
.end method

.method protected final d()V
    .locals 2

    const/4 v0, 0x0

    :goto_0
    iget v1, p0, Lcom/kamcord/android/core/KC_b;->a:I

    if-ge v0, v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_b;->b:[Lcom/kamcord/android/core/KC_y;

    aget-object v1, v1, v0

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_y;->a()V

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_0
    return-void
.end method
